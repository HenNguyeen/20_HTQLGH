using Microsoft.AspNetCore.Mvc;
using DeliveryManagementAPI.Models;
using DeliveryManagementAPI.Services;
using DeliveryManagementAPI.Services.Commands;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace DeliveryManagementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Yêu cầu authentication cho tất cả endpoints
    public class OrdersController : ControllerBase
    {
        private readonly OrderService _orderService;
        private readonly OrderCommandHandler _commandHandler;
        private readonly OrderStateService _orderStateService;
        private readonly DeliveryStaffService _staffService;
        private readonly ShippingFeeService _feeService;
        private readonly DeliveryDbContext _context;
        private readonly INotificationService _notificationService;
        private readonly ILogger<OrdersController> _logger;
        private readonly PaymentGatewayService _paymentGatewayService;

        public OrdersController(
            OrderService orderService,
            OrderCommandHandler commandHandler,
            OrderStateService orderStateService,
            DeliveryStaffService staffService,
            ShippingFeeService feeService,
            DeliveryDbContext context,
            INotificationService notificationService,
            ILogger<OrdersController> logger,
            PaymentGatewayService paymentGatewayService)
        {
            _orderService = orderService;
            _commandHandler = commandHandler;
            _orderStateService = orderStateService;
            _staffService = staffService;
            _feeService = feeService;
            _context = context;
            _notificationService = notificationService;
            _logger = logger;
            _paymentGatewayService = paymentGatewayService;
        }

        /// <summary>
        /// Lấy tất cả đơn hàng
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<Order>>> GetAllOrders()
        {
            try
            {
                var orders = await _orderService.GetAllOrdersAsync();
                return Ok(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all orders");
                return StatusCode(500, new { success = false, message = "Lỗi khi lấy danh sách đơn hàng: " + ex.Message });
            }
        }

        /// <summary>
        /// Lấy danh sách "đơn của tôi" theo UserId trong JWT
        /// </summary>
        [HttpGet("my")]
        [Authorize(Roles = "customer,admin,shipper")] // Bất kỳ ai đăng nhập đều có thể gọi để lấy đơn của chính mình
        public async Task<ActionResult<List<Order>>> GetMyOrders()
        {
            try
            {
                var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim))
                {
                    return Unauthorized(new { message = "Không xác định được người dùng từ token" });
                }

                if (!int.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized(new { message = "UserId trong token không hợp lệ" });
                }

                var orders = await _orderService.GetOrdersByCreatorAsync(userId);
                return Ok(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting my orders");
                return StatusCode(500, new { success = false, message = "Lỗi khi lấy danh sách đơn hàng của tôi: " + ex.Message });
            }
        }

        /// <summary>
        /// Lấy đơn hàng theo ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<Order>> GetOrderById(int id)
        {
            try
            {
                var order = await _orderService.GetOrderByIdAsync(id);
                
                if (order == null)
                {
                    return NotFound($"Không tìm thấy đơn hàng với ID: {id}");
                }

                return Ok(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting order by ID: {OrderId}", id);
                return StatusCode(500, new { success = false, message = "Lỗi khi lấy thông tin đơn hàng: " + ex.Message });
            }
        }

        /// <summary>
        /// Tạo đơn hàng mới (Nhận mã đơn hàng và hàng hóa)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "admin,customer")] // Chỉ admin và customer được tạo đơn
        public async Task<ActionResult<Order>> CreateOrder([FromBody] CreateOrderDto orderDto)
        {
            try
            {
                // ========== VALIDATION - ALL REQUIRED FIELDS ==========
                
                // NOTE: OrderCode validation is skipped - backend will auto-generate if empty
                
                // Validate Customer Name
                var customerNameValidation = OrderValidationHelper.ValidateCustomerName(orderDto.CustomerName);
                if (!customerNameValidation.IsValid)
                    return BadRequest(new { message = customerNameValidation.ErrorMessage, field = "customerName" });
                
                // Validate Phone Number
                var phoneValidation = OrderValidationHelper.ValidatePhoneNumber(orderDto.CustomerPhone);
                if (!phoneValidation.IsValid)
                    return BadRequest(new { message = phoneValidation.ErrorMessage, field = "phoneNumber" });
                
                // Validate Delivery Address
                var addressValidation = OrderValidationHelper.ValidateDeliveryAddress(orderDto.DeliveryAddress);
                if (!addressValidation.IsValid)
                    return BadRequest(new { message = addressValidation.ErrorMessage, field = "deliveryAddress" });
                
                // Validate Location (Ward, District, City)
                var locationValidation = OrderValidationHelper.ValidateLocation(orderDto.Ward, orderDto.District, orderDto.City);
                if (!locationValidation.IsValid)
                    return BadRequest(new { message = locationValidation.ErrorMessage, field = "location" });
                
                // Validate Product ID
                var productIdValidation = OrderValidationHelper.ValidateProductId(orderDto.ProductId);
                if (!productIdValidation.IsValid)
                    return BadRequest(new { message = productIdValidation.ErrorMessage, field = "productId" });
                
                // Validate Weight
                var weightValidation = OrderValidationHelper.ValidateWeight(orderDto.Weight.ToString());
                if (!weightValidation.IsValid)
                    return BadRequest(new { message = weightValidation.ErrorMessage, field = "weight" });
                
                // Validate Dimensions
                var dimensionsValidation = OrderValidationHelper.ValidateDimensions(
                    orderDto.Size?.Split('x')?[0]?.Trim(),
                    orderDto.Size?.Split('x')?[1]?.Trim(),
                    orderDto.Size?.Split('x')?[2]?.Trim()
                );
                if (!dimensionsValidation.IsValid)
                    return BadRequest(new { message = dimensionsValidation.ErrorMessage, field = "dimensions" });
                
                // Validate Distance
                var distanceValidation = OrderValidationHelper.ValidateDistance(orderDto.Distance.ToString());
                if (!distanceValidation.IsValid)
                    return BadRequest(new { message = distanceValidation.ErrorMessage, field = "distance" });
                
                // Validate Payment Method
                var paymentValidation = OrderValidationHelper.ValidatePaymentMethod(orderDto.PaymentMethod.ToString());
                if (!paymentValidation.IsValid)
                    return BadRequest(new { message = paymentValidation.ErrorMessage, field = "paymentMethod" });
                
                
                // Validate COD Amount (if applicable)
                var codValidation = OrderValidationHelper.ValidateCODAmount(
                    orderDto.CollectionAmount.ToString(),
                    orderDto.CollectMoney
                );
                if (!codValidation.IsValid)
                    return BadRequest(new { message = codValidation.ErrorMessage, field = "codAmount" });
                
                // Validate Note
                var noteValidation = OrderValidationHelper.ValidateNote(orderDto.Notes);
                if (!noteValidation.IsValid)
                    return BadRequest(new { message = noteValidation.ErrorMessage, field = "note" });
                
                // Check Code Uniqueness
                var existingOrder = await _context.Orders.FirstOrDefaultAsync(o => o.OrderCode == orderDto.OrderCode);
                if (existingOrder != null)
                    return BadRequest(new { message = "Mã đơn hàng đã tồn tại", field = "orderCode" });
                
                _logger.LogInformation($"[CreateOrder] All validations passed for order code: {orderDto.OrderCode}");
                
                // ========== CALCULATE SHIPPING FEE ==========
                
                var isFragile = orderDto.IsFragile;
                var isHighValue = orderDto.IsValuable;
                var isBulky = orderDto.IsVehicle;
                
                var shippingFee = OrderValidationHelper.CalculateShippingFee(
                    (decimal)orderDto.Distance,
                    (decimal)orderDto.Weight,
                    orderDto.DeliveryType.ToString(),
                    isFragile,
                    isHighValue,
                    isBulky
                );

                // Lấy UserId từ JWT để gắn người tạo đơn
                int? createdByUserId = null;
                var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(userIdClaim, out var uid))
                {
                    createdByUserId = uid;
                }

                // Nếu client không gửi OrderCode thì tạo mã đơn ở server
                string orderCode = orderDto.OrderCode;
                if (string.IsNullOrWhiteSpace(orderCode))
                {
                    // Format: DHyyyyMMddHHmmssfffNNN (NH: DH + timestamp + random 3 digits)
                    orderCode = $"DH{DateTime.Now:yyyyMMddHHmmssfff}{new Random().Next(100, 999)}";
                }

                // Tạo customer trước
                var customer = new Customer
                {
                    FullName = OrderValidationHelper.NormalizeInput(orderDto.CustomerName),
                    PhoneNumber = orderDto.CustomerPhone,
                    Address = OrderValidationHelper.NormalizeInput(orderDto.DeliveryAddress),
                    Ward = orderDto.Ward,
                    District = orderDto.District,
                    City = orderDto.City
                };
                _context.Customers.Add(customer);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation($"[CreateOrder] Customer created with ID: {customer.CustomerId}");

                // Tạo đơn hàng mới sử dụng Builder Pattern (Design Pattern 10)
                var order = new OrderBuilder()
                    .WithOrderCode(orderCode)
                    .CreatedBy(createdByUserId)
                    .ForCustomer(customer)
                    .FromDto(orderDto)
                    .WithPayment(orderDto.PaymentMethod, shippingFee)
                    .Build();

                // Auto-generate ProductCode if empty (set same as OrderCode for simplicity)
                if (string.IsNullOrEmpty(order.ProductCode))
                {
                    order.ProductCode = order.OrderCode;
                }

                _logger.LogInformation($"[CreateOrder] Order built: Code={order.OrderCode}, CustomerId={order.CustomerId}, ShippingFee={order.ShippingFee}");

                // Thực thi Command Pattern (Design Pattern 12) để tạo đơn hàng
                try
                {
                    var createCommand = new CreateOrderCommand(order);
                    await _commandHandler.ExecuteAsync(createCommand);
                    _logger.LogInformation($"[CreateOrder] Command executed successfully for order: {orderCode}");
                }
                catch (InvalidOperationException ex)
                {
                    _logger.LogError($"[CreateOrder] Validation error: {ex.Message}");
                    return BadRequest(new { success = false, message = $"Lỗi validate: {ex.Message}" });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"[CreateOrder] Error executing CreateOrderCommand for order: {orderCode}");
                    return StatusCode(500, new { success = false, message = $"Lỗi tạo đơn hàng: {ex.Message}" });
                }
                
                // Sau khi Command execute, order object đã có OrderId được populate từ DB
                var createdOrder = order;
                if (createdOrder == null || createdOrder.OrderId == 0)
                {
                    _logger.LogError($"[CreateOrder] Order ID not populated: {orderCode}");
                    return StatusCode(500, new { success = false, message = "Không thể tạo đơn hàng (ID không được generate)" });
                }
                
                _logger.LogInformation($"[CreateOrder] Order created successfully: ID={createdOrder.OrderId}, Code={createdOrder.OrderCode}");
                
                // Gửi thông báo đơn hàng mới
                try
                {
                    await _notificationService.SendOrderNotificationAsync(createdOrder.OrderId, "created");
                }
                catch (Exception notifEx)
                {
                    _logger.LogError(notifEx, $"Error sending notification for order {createdOrder.OrderId}");
                    // Không throw exception, vẫn trả về order thành công
                }
                
                // Xử lý thanh toán online qua Momo (Adapter Pattern #15)
                if (orderDto.PaymentMethod == PaymentMethod.Momo && !string.IsNullOrEmpty(orderDto.PaymentGateway))
                {
                    _logger.LogInformation("[Payment] Đơn hàng {OrderCode} thanh toán qua {Gateway}", orderCode, orderDto.PaymentGateway);
                    
                    try
                    {
                        // Tạo URL thanh toán qua gateway được chọn (VNPay, Momo, etc.)
                        var returnUrl = $"{Request.Scheme}://{Request.Host}/api/payment/callback?gateway={orderDto.PaymentGateway}";
                        var paymentResult = await _paymentGatewayService.ProcessPaymentAsync(
                            orderDto.PaymentGateway,
                            order.ShippingFee, // Thanh toán phí ship
                            orderCode,
                            $"Thanh toán đơn hàng {orderCode}",
                            returnUrl
                        );
                        
                        if (paymentResult.Success && !string.IsNullOrEmpty(paymentResult.PaymentUrl))
                        {
                            _logger.LogInformation("[Payment] ✅ Tạo payment URL thành công cho đơn {OrderCode}", orderCode);
                            
                            // Trả về order kèm payment URL để client redirect
                            return Ok(new {
                                success = true,
                                order = createdOrder,
                                payment = new {
                                    required = true,
                                    gateway = orderDto.PaymentGateway,
                                    paymentUrl = paymentResult.PaymentUrl,
                                    transactionId = paymentResult.TransactionId
                                },
                                message = "Đơn hàng đã được tạo. Vui lòng thanh toán để hoàn tất."
                            });
                        }
                        else
                        {
                            _logger.LogWarning("[Payment] ❌ Không tạo được payment URL: {Error}", paymentResult.ErrorMessage);
                            return BadRequest(new {
                                success = false,
                                message = $"Lỗi khi tạo link thanh toán: {paymentResult.ErrorMessage}"
                            });
                        }
                    }
                    catch (Exception paymentEx)
                    {
                        _logger.LogError(paymentEx, "[Payment] ❌ Lỗi xử lý payment cho đơn {OrderCode}", orderCode);
                        return StatusCode(500, new {
                            success = false,
                            message = "Lỗi khi kết nối với cổng thanh toán. Vui lòng thử lại."
                        });
                    }
                }
                
                // Thanh toán COD hoặc các phương thức khác - trả về order thống nhất
                return Ok(new {
                    success = true,
                    order = createdOrder,
                    payment = new {
                        required = false,
                        gateway = (string)null,
                        paymentUrl = (string)null,
                        transactionId = (string)null
                    },
                    message = "Đơn hàng đã được tạo thành công"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order");
                return StatusCode(500, new { success = false, message = "Lỗi khi tạo đơn hàng: " + ex.Message });
            }
        }

        /// <summary>
        /// Cập nhật trạng thái đơn hàng
        /// </summary>
        [HttpPatch("{id}/status")]
        [Authorize(Roles = "admin,shipper")] // Chỉ admin và shipper được cập nhật trạng thái
        public async Task<ActionResult<Order>> UpdateOrderStatus(
            int id, 
            [FromBody] UpdateOrderStatusDto statusDto)
        {
            try
            {
                var order = await _orderService.GetOrderByIdAsync(id);
                
                if (order == null)
                {
                    return NotFound($"Không tìm thấy đơn hàng với ID: {id}");
                }

                var previousStatus = order.Status;

                // Nếu shipper chuyển từ ChuaNhan sang DaNhanDangGiao trực tiếp, tự động gán shipper
                if (previousStatus == OrderStatus.ChuaNhan && statusDto.Status == OrderStatus.DaNhanDangGiao)
                {
                    int staffId = 0;
                    
                    if (!string.IsNullOrEmpty(statusDto.StaffId))
                    {
                        staffId = int.Parse(statusDto.StaffId);
                        order.AssignedStaffId = statusDto.StaffId;
                        order.AssignedStaff = await _staffService.GetStaffByIdAsync(staffId);
                        order.ReceivedDate = DateTime.Now;
                    }
                }

                // Thực thi Command Pattern (Design Pattern 12) để cập nhật trạng thái
                var updateStatusCommand = new UpdateOrderStatusCommand(id, statusDto.Status);
                await _commandHandler.ExecuteAsync(updateStatusCommand);

                // Cập nhật thông tin khác (ngoài trạng thái)
                order = await _orderService.GetOrderByIdAsync(id);
                
                switch (statusDto.Status)
                {
                    case OrderStatus.DaNhanChuaGiao:
                        if (!string.IsNullOrEmpty(statusDto.StaffId))
                        {
                            var staffId = int.Parse(statusDto.StaffId);
                            order.AssignedStaffId = statusDto.StaffId;
                            order.AssignedStaff = await _staffService.GetStaffByIdAsync(staffId);
                        }
                        break;
                }

                if (!string.IsNullOrEmpty(statusDto.Notes))
                {
                    order.Notes += $"\n{DateTime.Now:yyyy-MM-dd HH:mm}: {statusDto.Notes}";
                }

                await _orderService.UpdateOrderAsync(order);
                
                // Gửi thông báo cập nhật trạng thái
                try
                {
                    string eventType = statusDto.Status switch
                    {
                        OrderStatus.DaNhanChuaGiao => "confirmed",
                        OrderStatus.DaNhanDangGiao => "in_transit",
                        OrderStatus.DaGiao => "delivered",
                        _ => "status_changed"
                    };
                    
                    // Nếu là assigned staff thì gửi thêm thông báo assigned
                    if (!string.IsNullOrEmpty(statusDto.StaffId))
                    {
                        await _notificationService.SendOrderNotificationAsync(id, "assigned");
                    }
                    
                    await _notificationService.SendOrderNotificationAsync(id, eventType);
                }
                catch (Exception notifEx)
                {
                    _logger.LogError(notifEx, $"Error sending notification for order {id}");
                }
                
                return Ok(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order status: {OrderId}", id);
                return StatusCode(500, new { success = false, message = "Lỗi khi cập nhật trạng thái đơn hàng: " + ex.Message });
            }
        }

        /// <summary>
        /// Gán nhân viên giao hàng cho đơn hàng
        /// </summary>
        [HttpPatch("{id}/assign-staff/{staffId}")]
        [Authorize(Roles = "admin")] // Chỉ admin được gán nhân viên
        public async Task<ActionResult<Order>> AssignStaff(int id, int staffId)
        {
            try
            {
                // Thực thi Command Pattern (Design Pattern 12) để gán nhân viên
                var assignCommand = new AssignStaffCommand(id, staffId);
                await _commandHandler.ExecuteAsync(assignCommand);

                var order = await _orderService.GetOrderByIdAsync(id);

                // Gửi thông báo cho shipper và khách hàng khi đơn được gán.
                try
                {
                    await _notificationService.SendOrderNotificationAsync(id, "assigned");
                }
                catch (Exception notifEx)
                {
                    _logger.LogError(notifEx, "Error sending assigned notification for order {OrderId}", id);
                }

                return Ok(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning staff to order: {OrderId}", id);
                return StatusCode(500, new { success = false, message = "Lỗi khi gán nhân viên: " + ex.Message });
            }
        }

        /// <summary>
        /// Thanh toán đơn hàng (chỉ owner được thanh toán)
        /// </summary>
        [HttpPost("{id}/pay")]
        [Authorize(Roles = "customer")]
        public async Task<ActionResult<Order>> PayOrder(int id)
        {
            try
            {
                var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized(new { message = "Không xác định được người dùng từ token" });
                }

                var order = await _orderService.GetOrderByIdAsync(id);
                if (order == null)
                    return NotFound("Không tìm thấy đơn hàng");

                if (order.CreatedByUserId != userId)
                    return Forbid("Bạn chỉ có thể thanh toán đơn của chính mình");

                if (order.IsPaid)
                    return BadRequest("Đơn hàng đã được thanh toán trước đó");

                // Đánh dấu đã thanh toán
                order.IsPaid = true;
                order.PaidAmount = order.ShippingFee;
                order.PaymentTime = DateTime.Now;
                await _orderService.UpdateOrderAsync(order);

                return Ok(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error paying order: {OrderId}", id);
                return StatusCode(500, "Lỗi khi thanh toán đơn hàng");
            }
        }

        /// <summary>
        /// Lấy đơn hàng theo trạng thái
        /// </summary>
        [HttpGet("status/{status}")]
        public async Task<ActionResult<List<Order>>> GetOrdersByStatus(OrderStatus status)
        {
            try
            {
                var orders = await _orderService.GetOrdersByStatusAsync(status);
                return Ok(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting orders by status: {Status}", status);
                return StatusCode(500, "Lỗi khi lấy danh sách đơn hàng");
            }
        }

        /// <summary>
        /// Lấy đơn hàng theo nhân viên giao hàng
        /// </summary>
        [HttpGet("staff/{staffId}")]
        public async Task<ActionResult<List<Order>>> GetOrdersByStaff(int staffId)
        {
            try
            {
                var orders = await _orderService.GetOrdersByStaffIdAsync(staffId);
                return Ok(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting orders by staff: {StaffId}", staffId);
                return StatusCode(500, "Lỗi khi lấy danh sách đơn hàng");
            }
        }

        /// <summary>
        /// Xóa đơn hàng
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")] // Chỉ admin được xóa đơn hàng
        public async Task<ActionResult> DeleteOrder(int id)
        {
            try
            {
                // Thực thi Command Pattern (Design Pattern 12) để xóa đơn hàng
                var deleteCommand = new DeleteOrderCommand(id);
                await _commandHandler.ExecuteAsync(deleteCommand);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting order: {OrderId}", id);
                return StatusCode(500, new { success = false, message = "Lỗi khi xóa đơn hàng: " + ex.Message });
            }
        }

        /// <summary>
        /// Tạo nhiều đơn hàng từ Excel import
        /// </summary>
        [HttpPost("bulk")]
        [Authorize(Roles = "customer,admin")]
        public async Task<ActionResult> CreateBulkOrders([FromBody] List<Order> orders)
        {
            try
            {
                if (orders == null || orders.Count == 0)
                {
                    return BadRequest(new { message = "Danh sách đơn hàng trống" });
                }

                var createdOrders = new List<Order>();
                var errors = new List<string>();

                foreach (var order in orders)
                {
                    try
                    {
                        // Validate basic fields
                        if (order.Customer == null || 
                            string.IsNullOrEmpty(order.Customer.FullName) ||
                            string.IsNullOrEmpty(order.Customer.Address))
                        {
                            errors.Add($"Đơn hàng thiếu thông tin bắt buộc (khách hàng: {order.Customer?.FullName})");
                            continue;
                        }

                        order.CreatedDate = DateTime.Now;
                        order.Status = OrderStatus.ChuaNhan;
                        
                        _context.Orders.Add(order);
                        createdOrders.Add(order);
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"Lỗi tạo đơn {order.Customer?.FullName}: {ex.Message}");
                    }
                }

                await _context.SaveChangesAsync();

                return Ok(new 
                { 
                    success = createdOrders.Count,
                    failed = errors.Count,
                    errors = errors,
                    orders = createdOrders
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating bulk orders");
                return StatusCode(500, "Lỗi khi tạo đơn hàng hàng loạt: " + ex.Message);
            }
        }

        // ========== STATE PATTERN ENDPOINTS (Pattern #15) ==========

        /// <summary>
        /// Gán đơn hàng cho nhân viên giao hàng (State Pattern)
        /// </summary>
        [HttpPatch("{id}/state/assign-staff/{staffId}")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult> AssignStaffState(int id, string staffId)
        {
            try
            {
                var success = await _orderStateService.AssignOrderToStaffAsync(id, staffId);
                if (!success)
                    return BadRequest("Không thể gán đơn hàng ở trạng thái hiện tại");

                var order = await _orderService.GetOrderByIdAsync(id);
                await _notificationService.SendOrderNotificationAsync(id, "assigned");
                
                return Ok(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning staff via state: {OrderId}", id);
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Bắt đầu giao hàng (State Pattern)
        /// </summary>
        [HttpPatch("{id}/state/start-delivery")]
        [Authorize(Roles = "shipper,admin")]
        public async Task<ActionResult> StartDeliveryState(int id)
        {
            try
            {
                var success = await _orderStateService.StartDeliveryAsync(id);
                if (!success)
                    return BadRequest("Không thể bắt đầu giao ở trạng thái hiện tại");

                var order = await _orderService.GetOrderByIdAsync(id);
                await _notificationService.SendOrderNotificationAsync(id, "in-transit");
                
                return Ok(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting delivery via state: {OrderId}", id);
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Hoàn tất giao hàng (State Pattern)
        /// </summary>
        [HttpPatch("{id}/state/complete-delivery")]
        [Authorize(Roles = "shipper,admin")]
        public async Task<ActionResult> CompleteDeliveryState(int id)
        {
            try
            {
                var success = await _orderStateService.CompleteDeliveryAsync(id);
                if (!success)
                    return BadRequest("Không thể hoàn tát giao ở trạng thái hiện tại");

                var order = await _orderService.GetOrderByIdAsync(id);
                await _notificationService.SendOrderNotificationAsync(id, "delivered");
                
                return Ok(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing delivery via state: {OrderId}", id);
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Hủy đơn hàng (State Pattern)
        /// </summary>
        [HttpPatch("{id}/state/cancel")]
        [Authorize(Roles = "admin,customer")]
        public async Task<ActionResult> CancelOrderState(int id, [FromBody] CancelOrderRequest request)
        {
            try
            {
                var success = await _orderStateService.CancelOrderAsync(id, request?.Reason ?? "Hủy từ API");
                if (!success)
                    return BadRequest("Không thể hủy đơn hàng ở trạng thái hiện tại");

                var order = await _orderService.GetOrderByIdAsync(id);
                
                return Ok(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling order via state: {OrderId}", id);
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Lấy danh sách hành động được phép (State Pattern)
        /// GET /api/orders/123/state/allowed-actions
        /// </summary>
        [HttpGet("{id}/state/allowed-actions")]
        [Authorize]
        public async Task<ActionResult<List<string>>> GetAllowedActions(int id)
        {
            try
            {
                var allowedActions = await _orderStateService.GetAllowedActionsAsync(id);
                return Ok(allowedActions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting allowed actions: {OrderId}", id);
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Lấy thông tin chi tiết trạng thái hiện tại (State Pattern)
        /// GET /api/orders/123/state/current
        /// </summary>
        [HttpGet("{id}/state/current")]
        [Authorize]
        public async Task<ActionResult> GetCurrentState(int id)
        {
            try
            {
                var state = await _orderStateService.GetCurrentStateAsync(id);
                if (state == null)
                    return NotFound("Không tìm thấy đơn hàng");

                return Ok(new
                {
                    stateName = state.StateName,
                    status = state.Status,
                    allowedActions = state.GetAllowedActions()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current state: {OrderId}", id);
                return StatusCode(500, ex.Message);
            }
        }
    }

    /// <summary>
    /// DTO cho yêu cầu hủy đơn hàng
    /// </summary>
    public class CancelOrderRequest
    {
        public string? Reason { get; set; }
    }
}
