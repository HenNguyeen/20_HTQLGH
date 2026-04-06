using DeliveryManagementAPI.Models;
using DeliveryManagementAPI.Models.Momo;
using DeliveryManagementAPI.Services.Momo;

namespace DeliveryManagementAPI.Services
{
    /// <summary>
    /// Momo Adapter - Adapter Pattern #15
    /// Chuyển đổi interface riêng của Momo (Adaptee) thành interface chung IPaymentGateway (Target)
    /// Cho phép hệ thống sử dụng cả Momo và VNPay thông qua cùng một interface
    /// </summary>
    public class MomoAdapter : IPaymentGateway
    {
        private readonly MomoService _momoService;
        private readonly ILogger<MomoAdapter> _logger;

        public string GatewayName => "Momo";

        public MomoAdapter(MomoService momoService, ILogger<MomoAdapter> logger)
        {
            _momoService = momoService;
            _logger = logger;
        }

        /// <summary>
        /// Chuyển đổi từ interface chung sang interface Momo cụ thể
        /// </summary>
        public async Task<PaymentResult> ProcessPaymentAsync(
            decimal amount, 
            string orderCode, 
            string description, 
            string returnUrl)
        {
            _logger.LogInformation("[MomoAdapter] Đang xử lý thanh toán: {OrderCode}, Amount: {Amount}VND", 
                orderCode, amount);

            try
            {
                // ADAPT: Chuyển đổi từ format chung sang format Momo
                // Lưu ý: Momo dùng VND thẳng, không như VNPay phải nhân 100
                var momoRequest = new MomoPaymentRequest
                {
                    PartnerCode = "DEMO_PARTNER",
                    RequestId = Guid.NewGuid().ToString(),
                    OrderId = orderCode,
                    OrderInfo = description,
                    Amount = (long)amount, // Momo dùng đơn vị VND trực tiếp
                    RedirectUrl = returnUrl,
                    IpnUrl = returnUrl.Replace("/return", "/ipn"), // IPN callback URL
                    RequestType = "captureWallet",
                    ExtraData = "",
                    Lang = "vi"
                };

                // Gọi Momo service với interface riêng của nó
                var momoResponse = _momoService.CreatePayment(momoRequest);

                // ADAPT: Chuyển đổi MomoPaymentResponse sang PaymentResult chung
                // Lưu ý: Momo dùng ResultCode = 0 cho success, VNPay dùng "00"
                var result = new PaymentResult
                {
                    Success = momoResponse.ResultCode == 0,
                    TransactionId = momoResponse.RequestId,
                    PaymentUrl = momoResponse.PayUrl, // Momo gọi là PayUrl, VNPay gọi là PaymentUrl
                    ErrorMessage = momoResponse.ResultCode != 0 ? momoResponse.Message : null,
                    ErrorCode = momoResponse.ResultCode.ToString(),
                    AdditionalData = new Dictionary<string, string>
                    {
                        { "QrCodeUrl", momoResponse.QrCodeUrl },
                        { "Deeplink", momoResponse.Deeplink },
                        { "PartnerCode", momoResponse.PartnerCode }
                    },
                    TransactionTime = DateTime.Now
                };

                _logger.LogInformation("[MomoAdapter] ✅ Xử lý thành công: {Success}, RequestId: {RequestId}", 
                    result.Success, result.TransactionId);

                return await Task.FromResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[MomoAdapter] ❌ Lỗi xử lý thanh toán");
                return new PaymentResult
                {
                    Success = false,
                    ErrorMessage = ex.Message,
                    ErrorCode = "99"
                };
            }
        }

        /// <summary>
        /// Xác minh IPN callback từ Momo
        /// </summary>
        public async Task<PaymentResult> VerifyPaymentAsync(Dictionary<string, string> queryParams)
        {
            _logger.LogInformation("[MomoAdapter] Xác minh IPN callback");

            try
            {
                // ADAPT: Chuyển đổi dictionary sang MomoIpnRequest
                var ipnRequest = new MomoIpnRequest
                {
                    PartnerCode = queryParams.GetValueOrDefault("partnerCode", ""),
                    OrderId = queryParams.GetValueOrDefault("orderId", ""),
                    RequestId = queryParams.GetValueOrDefault("requestId", ""),
                    Amount = long.TryParse(queryParams.GetValueOrDefault("amount", "0"), out var amt) ? amt : 0,
                    ResultCode = long.TryParse(queryParams.GetValueOrDefault("resultCode", "99"), out var code) ? code : 99,
                    Message = queryParams.GetValueOrDefault("message", ""),
                    TransId = long.TryParse(queryParams.GetValueOrDefault("transId", "0"), out var tid) ? tid : 0,
                    TransactionType = queryParams.GetValueOrDefault("transactionType", ""),
                    Signature = queryParams.GetValueOrDefault("signature", "")
                };

                // Gọi Momo service để verify
                bool isValid = _momoService.VerifyIpn(ipnRequest);

                // ADAPT: Chuyển đổi sang PaymentResult chung
                var result = new PaymentResult
                {
                    Success = isValid && ipnRequest.ResultCode == 0,
                    TransactionId = ipnRequest.TransId.ToString(),
                    ErrorCode = ipnRequest.ResultCode.ToString(),
                    ErrorMessage = !isValid ? "Chữ ký không hợp lệ" : 
                                   ipnRequest.ResultCode != 0 ? $"Giao dịch thất bại: {ipnRequest.Message}" : null,
                    AdditionalData = new Dictionary<string, string>
                    {
                        { "OrderId", ipnRequest.OrderId },
                        { "Amount", ipnRequest.Amount.ToString() },
                        { "TransactionType", ipnRequest.TransactionType }
                    },
                    TransactionTime = DateTime.Now
                };

                _logger.LogInformation("[MomoAdapter] Verify result: {Success}, TransId: {TransId}", 
                    result.Success, result.TransactionId);

                return await Task.FromResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[MomoAdapter] ❌ Lỗi verify IPN");
                return new PaymentResult
                {
                    Success = false,
                    ErrorMessage = ex.Message,
                    ErrorCode = "99"
                };
            }
        }

        /// <summary>
        /// Hoàn tiền qua Momo
        /// </summary>
        public async Task<RefundResult> RefundAsync(string transactionId, decimal amount, string reason)
        {
            _logger.LogInformation("[MomoAdapter] Đang xử lý hoàn tiền: TransId: {TransId}, Amount: {Amount}VND", 
                transactionId, amount);

            try
            {
                // ADAPT: Chuyển đổi sang format Momo
                var refundRequest = new MomoRefundRequest
                {
                    PartnerCode = "DEMO_PARTNER",
                    RequestId = Guid.NewGuid().ToString(),
                    TransId = long.TryParse(transactionId, out var tid) ? tid : 0,
                    Amount = (long)amount, // Momo dùng VND trực tiếp
                    Description = reason
                };

                // Gọi Momo service
                var momoResponse = _momoService.ProcessRefund(refundRequest);

                // ADAPT: Chuyển đổi MomoRefundResponse sang RefundResult
                var result = new RefundResult
                {
                    Success = momoResponse.ResultCode == 0,
                    RefundId = momoResponse.RequestId,
                    RefundedAmount = amount,
                    ErrorMessage = momoResponse.ResultCode != 0 ? momoResponse.Message : null,
                    RefundTime = DateTime.Now
                };

                _logger.LogInformation("[MomoAdapter] Refund result: {Success}, RefundId: {RefundId}", 
                    result.Success, result.RefundId);

                return await Task.FromResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[MomoAdapter] ❌ Lỗi hoàn tiền");
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
        public async Task<PaymentResult> CheckTransactionStatusAsync(string transactionId)
        {
            _logger.LogInformation("[MomoAdapter] Kiểm tra trạng thái giao dịch: {TransId}", transactionId);

            // Trong thực tế sẽ gọi API query transaction của Momo
            // Demo: trả về transaction found
            return await Task.FromResult(new PaymentResult
            {
                Success = true,
                TransactionId = transactionId,
                ErrorMessage = null
            });
        }
    }
}
