using DeliveryManagementAPI.Models.VNPay;
using System.Security.Cryptography;
using System.Text;

namespace DeliveryManagementAPI.Services.VNPay
{
    /// <summary>
    /// VNPay Service - Adaptee trong Adapter Pattern #15
    /// Đây là service của bên thứ 3 với interface riêng, khác với interface chung của hệ thống
    /// Service này giả lập API của VNPay cho mục đích demo
    /// </summary>
    public class VNPayService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<VNPayService> _logger;

        // VNPay credentials (trong thực tế lấy từ config)
        private string TmnCode => _config["VNPay:TmnCode"] ?? "DEMO_TMNCODE";
        private string HashSecret => _config["VNPay:HashSecret"] ?? "DEMO_SECRET_KEY";
        private string BaseUrl => _config["VNPay:BaseUrl"] ?? "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html";

        public VNPayService(IConfiguration config, ILogger<VNPayService> logger)
        {
            _config = config;
            _logger = logger;
        }

        /// <summary>
        /// Tạo URL thanh toán VNPay (interface riêng của VNPay, khác với interface chung)
        /// </summary>
        public VNPayResponse CreatePaymentUrl(VNPayRequest request)
        {
            _logger.LogInformation("[VNPay] Tạo payment URL cho đơn hàng: {OrderId}, Amount: {Amount}", 
                request.OrderId, request.Amount);

            try
            {
                // Xây dựng query string theo format của VNPay
                var vnpayData = new SortedDictionary<string, string>
                {
                    { "vnp_Version", "2.1.0" },
                    { "vnp_Command", "pay" },
                    { "vnp_TmnCode", TmnCode },
                    { "vnp_Amount", request.Amount.ToString() }, // Đơn vị xu
                    { "vnp_CreateDate", request.CreateDate.ToString("yyyyMMddHHmmss") },
                    { "vnp_CurrCode", "VND" },
                    { "vnp_IpAddr", request.IpAddress },
                    { "vnp_Locale", "vn" },
                    { "vnp_OrderInfo", request.OrderDescription },
                    { "vnp_OrderType", request.OrderType },
                    { "vnp_ReturnUrl", request.ReturnUrl },
                    { "vnp_TxnRef", request.OrderId }
                };

                // Tạo chữ ký bảo mật (hash)
                var signData = string.Join("&", vnpayData.Select(kv => $"{kv.Key}={kv.Value}"));
                var secureHash = ComputeHmacSha512(HashSecret, signData);
                vnpayData.Add("vnp_SecureHash", secureHash);

                // Tạo URL
                var paymentUrl = BaseUrl + "?" + string.Join("&", vnpayData.Select(kv => $"{kv.Key}={Uri.EscapeDataString(kv.Value)}"));

                // Giả lập transaction ID
                var transactionId = $"VNPAY_{request.OrderId}_{DateTime.Now:yyyyMMddHHmmss}";

                _logger.LogInformation("[VNPay] ✅ Tạo payment URL thành công, TransactionId: {TransactionId}", transactionId);

                return new VNPayResponse
                {
                    ResponseCode = "00",
                    TransactionId = transactionId,
                    PaymentUrl = paymentUrl,
                    Message = "Success",
                    RawData = new Dictionary<string, string>(vnpayData)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[VNPay] ❌ Lỗi tạo payment URL");
                return new VNPayResponse
                {
                    ResponseCode = "99",
                    Message = ex.Message
                };
            }
        }

        /// <summary>
        /// Xác minh callback từ VNPay (interface riêng của VNPay)
        /// </summary>
        public VNPayVerifyResult VerifyPayment(Dictionary<string, string> vnpayData)
        {
            _logger.LogInformation("[VNPay] Xác minh payment callback");

            try
            {
                // Lấy secure hash từ VNPay gửi về
                if (!vnpayData.TryGetValue("vnp_SecureHash", out var vnpSecureHash))
                {
                    return new VNPayVerifyResult { IsValid = false, ResponseCode = "97" };
                }

                // Xóa secure hash khỏi data để verify
                var dataToVerify = new SortedDictionary<string, string>(vnpayData);
                dataToVerify.Remove("vnp_SecureHash");
                dataToVerify.Remove("vnp_SecureHashType");

                // Tính toán hash
                var signData = string.Join("&", dataToVerify.Select(kv => $"{kv.Key}={kv.Value}"));
                var computedHash = ComputeHmacSha512(HashSecret, signData);

                // So sánh hash (trong demo này, ta giả lập là hợp lệ)
                bool isValid = true; // Trong thực tế: computedHash.Equals(vnpSecureHash, StringComparison.OrdinalIgnoreCase);

                var result = new VNPayVerifyResult
                {
                    IsValid = isValid,
                    ResponseCode = vnpayData.GetValueOrDefault("vnp_ResponseCode", "99"),
                    TransactionId = vnpayData.GetValueOrDefault("vnp_TransactionNo", ""),
                    Amount = long.TryParse(vnpayData.GetValueOrDefault("vnp_Amount", "0"), out var amt) ? amt : 0,
                    OrderId = vnpayData.GetValueOrDefault("vnp_TxnRef", ""),
                    BankCode = vnpayData.GetValueOrDefault("vnp_BankCode", ""),
                    CardType = vnpayData.GetValueOrDefault("vnp_CardType", ""),
                    TransactionDate = DateTime.Now
                };

                _logger.LogInformation("[VNPay] Verify result: {IsValid}, ResponseCode: {ResponseCode}", 
                    isValid, result.ResponseCode);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[VNPay] ❌ Lỗi verify payment");
                return new VNPayVerifyResult { IsValid = false, ResponseCode = "99" };
            }
        }

        /// <summary>
        /// Hoàn tiền (interface riêng của VNPay)
        /// </summary>
        public VNPayRefundResponse ProcessRefund(VNPayRefundRequest request)
        {
            _logger.LogInformation("[VNPay] Xử lý hoàn tiền: {TransactionId}, Amount: {Amount}", 
                request.TransactionId, request.RefundAmount);

            // Giả lập API call hoàn tiền
            // Trong thực tế sẽ gọi API VNPay refund endpoint

            return new VNPayRefundResponse
            {
                ResponseCode = "00",
                RefundId = $"RF_{request.TransactionId}_{DateTime.Now:yyyyMMddHHmmss}",
                RefundAmount = request.RefundAmount,
                Message = "Refund successful"
            };
        }

        /// <summary>
        /// Tính toán HMAC SHA512 (theo yêu cầu của VNPay)
        /// </summary>
        private string ComputeHmacSha512(string key, string data)
        {
            using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(key));
            var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }
    }
}
