using DeliveryManagementAPI.Models;
using DeliveryManagementAPI.Models.VNPay;
using DeliveryManagementAPI.Services.VNPay;

namespace DeliveryManagementAPI.Services
{
    /// <summary>
    /// VNPay Adapter - Adapter Pattern #15
    /// Chuyển đổi interface riêng của VNPay (Adaptee) thành interface chung IPaymentGateway (Target)
    /// Cho phép hệ thống sử dụng VNPay mà không phụ thuộc vào API cụ thể của nó
    /// </summary>
    public class VNPayAdapter : IPaymentGateway
    {
        private readonly VNPayService _vnPayService;
        private readonly ILogger<VNPayAdapter> _logger;

        public string GatewayName => "VNPay";

        public VNPayAdapter(VNPayService vnPayService, ILogger<VNPayAdapter> logger)
        {
            _vnPayService = vnPayService;
            _logger = logger;
        }

        /// <summary>
        /// Chuyển đổi từ interface chung sang interface VNPay cụ thể
        /// </summary>
        public async Task<PaymentResult> ProcessPaymentAsync(
            decimal amount, 
            string orderCode, 
            string description, 
            string returnUrl)
        {
            _logger.LogInformation("[VNPayAdapter] Đang xử lý thanh toán: {OrderCode}, Amount: {Amount}VND", 
                orderCode, amount);

            try
            {
                // ADAPT: Chuyển đổi từ format chung sang format VNPay
                var vnpayRequest = new VNPayRequest
                {
                    OrderId = orderCode,
                    Amount = (long)(amount * 100), // VNPay yêu cầu đơn vị xu (VND * 100)
                    OrderDescription = description,
                    OrderType = "billpayment",
                    ReturnUrl = returnUrl,
                    IpAddress = "127.0.0.1", // Trong thực tế lấy từ HttpContext
                    CreateDate = DateTime.Now
                };

                // Gọi service VNPay với interface riêng của nó
                var vnpayResponse = _vnPayService.CreatePaymentUrl(vnpayRequest);

                // ADAPT: Chuyển đổi từ VNPayResponse sang PaymentResult chung
                var result = new PaymentResult
                {
                    Success = vnpayResponse.ResponseCode == "00",
                    TransactionId = vnpayResponse.TransactionId,
                    PaymentUrl = vnpayResponse.PaymentUrl,
                    ErrorMessage = vnpayResponse.ResponseCode != "00" ? vnpayResponse.Message : null,
                    ErrorCode = vnpayResponse.ResponseCode,
                    AdditionalData = vnpayResponse.RawData,
                    TransactionTime = DateTime.Now
                };

                _logger.LogInformation("[VNPayAdapter] ✅ Xử lý thành công: {Success}, TransactionId: {TransactionId}", 
                    result.Success, result.TransactionId);

                return await Task.FromResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[VNPayAdapter] ❌ Lỗi xử lý thanh toán");
                return new PaymentResult
                {
                    Success = false,
                    ErrorMessage = ex.Message,
                    ErrorCode = "99"
                };
            }
        }

        /// <summary>
        /// Xác minh callback từ VNPay
        /// </summary>
        public async Task<PaymentResult> VerifyPaymentAsync(Dictionary<string, string> queryParams)
        {
            _logger.LogInformation("[VNPayAdapter] Xác minh payment callback");

            try
            {
                // Gọi VNPay service để verify
                var verifyResult = _vnPayService.VerifyPayment(queryParams);

                // ADAPT: Chuyển đổi VNPayVerifyResult sang PaymentResult
                var result = new PaymentResult
                {
                    Success = verifyResult.IsValid && verifyResult.ResponseCode == "00",
                    TransactionId = verifyResult.TransactionId,
                    ErrorCode = verifyResult.ResponseCode,
                    ErrorMessage = !verifyResult.IsValid ? "Chữ ký không hợp lệ" : 
                                   verifyResult.ResponseCode != "00" ? $"Giao dịch thất bại: {verifyResult.ResponseCode}" : null,
                    AdditionalData = new Dictionary<string, string>
                    {
                        { "OrderId", verifyResult.OrderId },
                        { "Amount", (verifyResult.Amount / 100m).ToString() }, // Chuyển từ xu về VND
                        { "BankCode", verifyResult.BankCode },
                        { "CardType", verifyResult.CardType }
                    },
                    TransactionTime = verifyResult.TransactionDate
                };

                _logger.LogInformation("[VNPayAdapter] Verify result: {Success}", result.Success);
                return await Task.FromResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[VNPayAdapter] ❌ Lỗi verify payment");
                return new PaymentResult
                {
                    Success = false,
                    ErrorMessage = ex.Message,
                    ErrorCode = "99"
                };
            }
        }

        /// <summary>
        /// Hoàn tiền qua VNPay
        /// </summary>
        public async Task<RefundResult> RefundAsync(string transactionId, decimal amount, string reason)
        {
            _logger.LogInformation("[VNPayAdapter] Đang xử lý hoàn tiền: {TransactionId}, Amount: {Amount}VND", 
                transactionId, amount);

            try
            {
                // ADAPT: Chuyển đổi sang format VNPay
                var refundRequest = new VNPayRefundRequest
                {
                    TransactionId = transactionId,
                    RefundAmount = (long)(amount * 100), // Chuyển sang xu
                    Reason = reason,
                    RefundDate = DateTime.Now
                };

                // Gọi VNPay service
                var vnpayRefundResponse = _vnPayService.ProcessRefund(refundRequest);

                // ADAPT: Chuyển đổi VNPayRefundResponse sang RefundResult
                var result = new RefundResult
                {
                    Success = vnpayRefundResponse.ResponseCode == "00",
                    RefundId = vnpayRefundResponse.RefundId,
                    RefundedAmount = vnpayRefundResponse.RefundAmount / 100m, // Chuyển từ xu về VND
                    ErrorMessage = vnpayRefundResponse.ResponseCode != "00" ? vnpayRefundResponse.Message : null,
                    RefundTime = DateTime.Now
                };

                _logger.LogInformation("[VNPayAdapter] Refund result: {Success}, RefundId: {RefundId}", 
                    result.Success, result.RefundId);

                return await Task.FromResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[VNPayAdapter] ❌ Lỗi hoàn tiền");
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
            _logger.LogInformation("[VNPayAdapter] Kiểm tra trạng thái giao dịch: {TransactionId}", transactionId);

            // Trong thực tế sẽ gọi API query transaction của VNPay
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
