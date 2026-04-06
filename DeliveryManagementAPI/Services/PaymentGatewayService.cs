using DeliveryManagementAPI.Models;

namespace DeliveryManagementAPI.Services
{
    /// <summary>
    /// Payment Gateway Service - Client trong Adapter Pattern #15
    /// Sử dụng các payment gateway thông qua interface chung IPaymentGateway
    /// Không phụ thuộc vào implementation cụ thể của VNPay, Momo hay bất kỳ gateway nào
    /// </summary>
    public class PaymentGatewayService
    {
        private readonly Dictionary<string, IPaymentGateway> _gateways;
        private readonly ILogger<PaymentGatewayService> _logger;

        public PaymentGatewayService(
            IEnumerable<IPaymentGateway> gateways,
            ILogger<PaymentGatewayService> logger)
        {
            // Tự động map các gateway theo tên
            _gateways = gateways.ToDictionary(g => g.GatewayName, StringComparer.OrdinalIgnoreCase);
            _logger = logger;

            _logger.LogInformation("[PaymentGatewayService] Đã đăng ký {Count} payment gateways: {Gateways}", 
                _gateways.Count, string.Join(", ", _gateways.Keys));
        }

        /// <summary>
        /// Xử lý thanh toán qua gateway được chọn
        /// </summary>
        /// <param name="gatewayName">Tên gateway (VNPay, Momo, etc.)</param>
        /// <param name="amount">Số tiền</param>
        /// <param name="orderCode">Mã đơn hàng</param>
        /// <param name="description">Mô tả</param>
        /// <param name="returnUrl">URL trả về</param>
        public async Task<PaymentResult> ProcessPaymentAsync(
            string gatewayName,
            decimal amount,
            string orderCode,
            string description,
            string returnUrl)
        {
            _logger.LogInformation("[PaymentGatewayService] Xử lý payment qua {Gateway}: {OrderCode}, {Amount}VND", 
                gatewayName, orderCode, amount);

            try
            {
                // Lấy gateway adapter phù hợp
                if (!_gateways.TryGetValue(gatewayName, out var gateway))
                {
                    var availableGateways = string.Join(", ", _gateways.Keys);
                    _logger.LogWarning("[PaymentGatewayService] ❌ Gateway không hợp lệ: {Gateway}. Available: {Available}", 
                        gatewayName, availableGateways);

                    return new PaymentResult
                    {
                        Success = false,
                        ErrorMessage = $"Payment gateway '{gatewayName}' không được hỗ trợ. Available gateways: {availableGateways}",
                        ErrorCode = "GATEWAY_NOT_FOUND"
                    };
                }

                // Gọi gateway thông qua interface chung - không cần biết là VNPay hay Momo
                var result = await gateway.ProcessPaymentAsync(amount, orderCode, description, returnUrl);

                _logger.LogInformation("[PaymentGatewayService] Payment result: {Success}, Gateway: {Gateway}", 
                    result.Success, gatewayName);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[PaymentGatewayService] ❌ Lỗi xử lý payment");
                return new PaymentResult
                {
                    Success = false,
                    ErrorMessage = ex.Message,
                    ErrorCode = "INTERNAL_ERROR"
                };
            }
        }

        /// <summary>
        /// Xác minh callback/return từ payment gateway
        /// </summary>
        public async Task<PaymentResult> VerifyPaymentAsync(string gatewayName, Dictionary<string, string> queryParams)
        {
            _logger.LogInformation("[PaymentGatewayService] Verify payment từ {Gateway}", gatewayName);

            try
            {
                if (!_gateways.TryGetValue(gatewayName, out var gateway))
                {
                    return new PaymentResult
                    {
                        Success = false,
                        ErrorMessage = $"Gateway '{gatewayName}' không tồn tại",
                        ErrorCode = "GATEWAY_NOT_FOUND"
                    };
                }

                return await gateway.VerifyPaymentAsync(queryParams);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[PaymentGatewayService] ❌ Lỗi verify payment");
                return new PaymentResult
                {
                    Success = false,
                    ErrorMessage = ex.Message,
                    ErrorCode = "INTERNAL_ERROR"
                };
            }
        }

        /// <summary>
        /// Hoàn tiền qua gateway
        /// </summary>
        public async Task<RefundResult> RefundAsync(
            string gatewayName,
            string transactionId,
            decimal amount,
            string reason)
        {
            _logger.LogInformation("[PaymentGatewayService] Refund qua {Gateway}: {TransId}, {Amount}VND", 
                gatewayName, transactionId, amount);

            try
            {
                if (!_gateways.TryGetValue(gatewayName, out var gateway))
                {
                    return new RefundResult
                    {
                        Success = false,
                        ErrorMessage = $"Gateway '{gatewayName}' không tồn tại"
                    };
                }

                return await gateway.RefundAsync(transactionId, amount, reason);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[PaymentGatewayService] ❌ Lỗi refund");
                return new RefundResult
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        /// <summary>
        /// Kiểm tra trạng thái giao dịch
        /// </summary>
        public async Task<PaymentResult> CheckTransactionStatusAsync(string gatewayName, string transactionId)
        {
            _logger.LogInformation("[PaymentGatewayService] Check status từ {Gateway}: {TransId}", 
                gatewayName, transactionId);

            try
            {
                if (!_gateways.TryGetValue(gatewayName, out var gateway))
                {
                    return new PaymentResult
                    {
                        Success = false,
                        ErrorMessage = $"Gateway '{gatewayName}' không tồn tại",
                        ErrorCode = "GATEWAY_NOT_FOUND"
                    };
                }

                return await gateway.CheckTransactionStatusAsync(transactionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[PaymentGatewayService] ❌ Lỗi check status");
                return new PaymentResult
                {
                    Success = false,
                    ErrorMessage = ex.Message,
                    ErrorCode = "INTERNAL_ERROR"
                };
            }
        }

        /// <summary>
        /// Lấy danh sách gateway khả dụng
        /// </summary>
        public List<string> GetAvailableGateways()
        {
            return _gateways.Keys.ToList();
        }
    }
}
