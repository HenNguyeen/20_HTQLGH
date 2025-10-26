using Microsoft.AspNetCore.Mvc;
using DeliveryManagementAPI.Models;
using DeliveryManagementAPI.Services;
using Microsoft.AspNetCore.Authorization;

namespace DeliveryManagementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Yêu cầu authentication cho tất cả endpoints
    public class OrdersController : ControllerBase
    {
        private readonly OrderService _orderService;
        private readonly DeliveryStaffService _staffService;
        private readonly ShippingFeeService _feeService;
        private readonly DeliveryDbContext _context;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(
            OrderService orderService,
            DeliveryStaffService staffService,
            ShippingFeeService feeService,
            DeliveryDbContext context,
            ILogger<OrdersController> logger)
        {
            _orderService = orderService;
            _staffService = staffService;
            _feeService = feeService;
            _context = context;
            _logger = logger;
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
                return StatusCode(500, "Lỗi khi lấy danh sách đơn hàng");
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
                return StatusCode(500, "Lỗi khi lấy danh sách đơn hàng của tôi");
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
                return StatusCode(500, "Lỗi khi lấy thông tin đơn hàng");
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
                // Tính phí giao hàng
                var shippingFee = _feeService.CalculateShippingFee(orderDto);

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
                    FullName = orderDto.CustomerName,
                    PhoneNumber = orderDto.CustomerPhone,
                    Address = orderDto.DeliveryAddress,
                    Ward = orderDto.Ward,
                    District = orderDto.District,
                    City = orderDto.City
                };
                _context.Customers.Add(customer);
                await _context.SaveChangesAsync();

                // Tạo đơn hàng mới
                var order = new Order
                {
                    OrderCode = orderCode,
                    CreatedDate = DateTime.Now,
                    CreatedByUserId = createdByUserId,
                    CustomerId = customer.CustomerId,
                    Customer = customer,
                    
                    ProductCode = orderDto.ProductCode,
                    PackageType = orderDto.PackageType,
                    Weight = orderDto.Weight,
                    Size = orderDto.Size,
                    Distance = orderDto.Distance,
                    
                    IsFragile = orderDto.IsFragile,
                    IsValuable = orderDto.IsValuable,
                    IsVehicle = orderDto.IsVehicle,
                    CollectMoney = orderDto.CollectMoney,
                    CollectionAmount = orderDto.CollectionAmount,
                    
                    PaymentMethod = orderDto.PaymentMethod,
                    ShippingFee = shippingFee,
                    IsPaid = orderDto.PaymentMethod == PaymentMethod.GuiNhanh || 
                             orderDto.PaymentMethod == PaymentMethod.ChuyenKhoan ||
                             orderDto.PaymentMethod == PaymentMethod.ThanhToanTrucTuyen,
                    
                    DeliveryType = orderDto.DeliveryType,
                    Status = OrderStatus.ChuaNhan,
                    Notes = orderDto.Notes
                };

                var createdOrder = await _orderService.AddOrderAsync(order);
                
                return CreatedAtAction(
                    nameof(GetOrderById), 
                    new { id = createdOrder.OrderId }, 
                    createdOrder);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order");
                return StatusCode(500, "Lỗi khi tạo đơn hàng");
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

                // Cập nhật trạng thái
                order.Status = statusDto.Status;
                
                // Cập nhật thông tin theo trạng thái
                switch (statusDto.Status)
                {
                    case OrderStatus.DaNhanChuaGiao:
                        order.ReceivedDate = DateTime.Now;
                        if (!string.IsNullOrEmpty(statusDto.StaffId))
                        {
                            var staffId = int.Parse(statusDto.StaffId);
                            order.AssignedStaffId = statusDto.StaffId;
                            order.AssignedStaff = await _staffService.GetStaffByIdAsync(staffId);
                        }
                        break;
                    
                    case OrderStatus.DaNhanDangGiao:
                        order.DeliveryStartDate = DateTime.Now;
                        break;
                    
                    case OrderStatus.DaGiao:
                        order.DeliveredDate = DateTime.Now;
                        break;
                }

                if (!string.IsNullOrEmpty(statusDto.Notes))
                {
                    order.Notes += $"\n{DateTime.Now:yyyy-MM-dd HH:mm}: {statusDto.Notes}";
                }

                var updatedOrder = await _orderService.UpdateOrderAsync(order);
                return Ok(updatedOrder);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order status: {OrderId}", id);
                return StatusCode(500, "Lỗi khi cập nhật trạng thái đơn hàng");
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
                var success = await _orderService.AssignStaffAsync(id, staffId);
                
                if (!success)
                {
                    return NotFound("Không tìm thấy đơn hàng hoặc nhân viên");
                }

                var order = await _orderService.GetOrderByIdAsync(id);
                return Ok(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning staff to order: {OrderId}", id);
                return StatusCode(500, "Lỗi khi gán nhân viên");
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
                var result = await _orderService.DeleteOrderAsync(id);
                
                if (!result)
                {
                    return NotFound($"Không tìm thấy đơn hàng với ID: {id}");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting order: {OrderId}", id);
                return StatusCode(500, "Lỗi khi xóa đơn hàng");
            }
        }
    }
}
