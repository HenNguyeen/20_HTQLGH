using Microsoft.AspNetCore.Mvc;
using DeliveryManagementAPI.Services;
using Microsoft.AspNetCore.Authorization;

namespace DeliveryManagementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly PaymentGatewayService _paymentGatewayService;
        private readonly OrderService _orderService;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(
            PaymentGatewayService paymentGatewayService,
            OrderService orderService,
            ILogger<PaymentController> logger)
        {
            _paymentGatewayService = paymentGatewayService;
            _orderService = orderService;
            _logger = logger;
        }

        /// <summary>
        /// Callback từ VNPay/Momo sau khi thanh toán
        /// URL: /api/payment/callback?gateway={VNPay|Momo}&...
        /// </summary>
        [HttpGet("callback")]
        [AllowAnonymous] // Cho phép VNPay/Momo gọi mà không cần auth
        public async Task<IActionResult> PaymentCallback([FromQuery] string gateway)
        {
            _logger.LogInformation("[Payment] Nhận callback từ {Gateway}", gateway);

            try
            {
                // Lấy tất cả query params
                var queryParams = Request.Query.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.ToString()
                );

                // Xác minh payment qua gateway tương ứng
                var verifyResult = await _paymentGatewayService.VerifyPaymentAsync(gateway, queryParams);

                if (verifyResult.Success)
                {
                    _logger.LogInformation("[Payment] ✅ Thanh toán thành công: {TransactionId}", verifyResult.TransactionId);

                    // Lấy OrderCode từ callback data
                    var orderCode = verifyResult.AdditionalData?.GetValueOrDefault("OrderId", "");
                    
                    if (!string.IsNullOrEmpty(orderCode))
                    {
                        // Cập nhật trạng thái thanh toán của Order
                        // TODO: Thêm field PaymentStatus vào Order model và update ở đây
                        _logger.LogInformation("[Payment] Đơn hàng {OrderCode} đã thanh toán", orderCode);
                    }

                    // Redirect về trang thành công
                    var successUrl = $"/payment-success.html?orderId={orderCode}&transactionId={verifyResult.TransactionId}";
                    return Redirect(successUrl);
                }
                else
                {
                    _logger.LogWarning("[Payment] ❌ Thanh toán thất bại: {Error}", verifyResult.ErrorMessage);

                    // Redirect về trang thất bại
                    var failUrl = $"/payment-failed.html?error={Uri.EscapeDataString(verifyResult.ErrorMessage ?? "Unknown error")}";
                    return Redirect(failUrl);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Payment] Lỗi xử lý callback từ {Gateway}", gateway);
                return Redirect("/payment-failed.html?error=system_error");
            }
        }

        /// <summary>
        /// IPN (Instant Payment Notification) từ payment gateway
        /// Webhook để gateway thông báo kết quả thanh toán (không qua browser)
        /// </summary>
        [HttpPost("ipn")]
        [AllowAnonymous]
        public async Task<IActionResult> PaymentIpn([FromQuery] string gateway)
        {
            _logger.LogInformation("[Payment IPN] Nhận IPN từ {Gateway}", gateway);

            try
            {
                // Lấy tất cả query params hoặc form data
                var queryParams = Request.Query.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.ToString()
                );

                // Nếu không có query params, thử lấy từ form
                if (!queryParams.Any() && Request.HasFormContentType)
                {
                    queryParams = Request.Form.ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value.ToString()
                    );
                }

                // Xác minh IPN
                var verifyResult = await _paymentGatewayService.VerifyPaymentAsync(gateway, queryParams);

                if (verifyResult.Success)
                {
                    _logger.LogInformation("[Payment IPN] ✅ IPN hợp lệ: {TransactionId}", verifyResult.TransactionId);

                    var orderCode = verifyResult.AdditionalData?.GetValueOrDefault("OrderId", "");
                    
                    if (!string.IsNullOrEmpty(orderCode))
                    {
                        // Cập nhật database
                        _logger.LogInformation("[Payment IPN] Cập nhật thanh toán cho đơn {OrderCode}", orderCode);
                        // TODO: Update Order payment status
                    }

                    // Trả về response để gateway biết đã nhận IPN
                    return Ok(new { 
                        success = true, 
                        message = "IPN received successfully" 
                    });
                }
                else
                {
                    _logger.LogWarning("[Payment IPN] ❌ IPN không hợp lệ: {Error}", verifyResult.ErrorMessage);
                    return BadRequest(new { 
                        success = false, 
                        message = verifyResult.ErrorMessage 
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Payment IPN] Lỗi xử lý IPN từ {Gateway}", gateway);
                return StatusCode(500, new { 
                    success = false, 
                    message = "Internal server error" 
                });
            }
        }

        /// <summary>
        /// Lấy danh sách payment gateway khả dụng
        /// </summary>
        [HttpGet("gateways")]
        [AllowAnonymous]
        public IActionResult GetAvailableGateways()
        {
            var gateways = _paymentGatewayService.GetAvailableGateways();
            return Ok(new {
                success = true,
                gateways = gateways
            });
        }

        /// <summary>
        /// Kiểm tra trạng thái giao dịch
        /// </summary>
        [HttpGet("transaction/{transactionId}")]
        [Authorize]
        public async Task<IActionResult> CheckTransactionStatus(
            [FromRoute] string transactionId,
            [FromQuery] string gateway)
        {
            try
            {
                var result = await _paymentGatewayService.CheckTransactionStatusAsync(gateway, transactionId);
                return Ok(new {
                    success = true,
                    transaction = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi kiểm tra transaction {TransactionId}", transactionId);
                return StatusCode(500, "Lỗi khi kiểm tra trạng thái giao dịch");
            }
        }
    }
}
