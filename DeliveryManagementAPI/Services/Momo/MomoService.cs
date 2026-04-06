using DeliveryManagementAPI.Models.Momo;
using System.Security.Cryptography;
using System.Text;

namespace DeliveryManagementAPI.Services.Momo
{
    /// <summary>
    /// Momo Service - Adaptee thứ 2 trong Adapter Pattern #15
    /// Service của bên thứ 3 Momo với interface hoàn toàn khác VNPay
    /// Giả lập API của Momo cho mục đích demo
    /// </summary>
    public class MomoService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<MomoService> _logger;

        // Momo credentials
        private string PartnerCode => _config["Momo:PartnerCode"] ?? "DEMO_PARTNER";
        private string AccessKey => _config["Momo:AccessKey"] ?? "DEMO_ACCESS_KEY";
        private string SecretKey => _config["Momo:SecretKey"] ?? "DEMO_SECRET_KEY";
        private string Endpoint => _config["Momo:Endpoint"] ?? "https://test-payment.momo.vn/v2/gateway/api/create";

        public MomoService(IConfiguration config, ILogger<MomoService> logger)
        {
            _config = config;
            _logger = logger;
        }

        /// <summary>
        /// Tạo payment request với Momo (interface khác với VNPay)
        /// </summary>
        public MomoPaymentResponse CreatePayment(MomoPaymentRequest request)
        {
            _logger.LogInformation("[Momo] Tạo payment cho OrderId: {OrderId}, Amount: {Amount}VND", 
                request.OrderId, request.Amount);

            try
            {
                // Tạo signature theo format của Momo (khác với VNPay)
                var rawSignature = $"accessKey={AccessKey}&amount={request.Amount}&extraData={request.ExtraData}" +
                                  $"&ipnUrl={request.IpnUrl}&orderId={request.OrderId}&orderInfo={request.OrderInfo}" +
                                  $"&partnerCode={request.PartnerCode}&redirectUrl={request.RedirectUrl}" +
                                  $"&requestId={request.RequestId}&requestType={request.RequestType}";

                request.Signature = ComputeHmacSha256(SecretKey, rawSignature);

                // Giả lập response từ Momo
                var transId = $"MOMO_{request.OrderId}_{DateTime.Now:yyyyMMddHHmmss}";
                var payUrl = $"https://test-payment.momo.vn/gw_payment/transactionProcessor?partnerCode={PartnerCode}&orderId={request.OrderId}";

                _logger.LogInformation("[Momo] ✅ Tạo payment thành công, TransId: {TransId}", transId);

                return new MomoPaymentResponse
                {
                    PartnerCode = request.PartnerCode,
                    RequestId = request.RequestId,
                    OrderId = request.OrderId,
                    Amount = request.Amount,
                    ResultCode = 0, // Momo dùng 0 cho success, khác với VNPay dùng "00"
                    Message = "Success",
                    PayUrl = payUrl, // Tên field khác với VNPay (PaymentUrl)
                    QrCodeUrl = $"{payUrl}&qr=1",
                    Deeplink = $"momo://payment?orderId={request.OrderId}"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Momo] ❌ Lỗi tạo payment");
                return new MomoPaymentResponse
                {
                    ResultCode = 99,
                    Message = ex.Message
                };
            }
        }

        /// <summary>
        /// Xử lý IPN callback từ Momo (format khác với VNPay)
        /// </summary>
        public bool VerifyIpn(MomoIpnRequest ipn)
        {
            _logger.LogInformation("[Momo] Verify IPN cho OrderId: {OrderId}", ipn.OrderId);

            try
            {
                // Tạo signature để verify
                var rawSignature = $"accessKey={AccessKey}&amount={ipn.Amount}&extraData=&message={ipn.Message}" +
                                  $"&orderId={ipn.OrderId}&orderInfo=&orderType=momo_wallet" +
                                  $"&partnerCode={ipn.PartnerCode}&payType=qr&requestId={ipn.RequestId}" +
                                  $"&responseTime=&resultCode={ipn.ResultCode}&transId={ipn.TransId}";

                var computedSignature = ComputeHmacSha256(SecretKey, rawSignature);

                // Trong demo, giả lập là hợp lệ
                bool isValid = true; // Thực tế: computedSignature == ipn.Signature

                _logger.LogInformation("[Momo] Verify IPN: {IsValid}, ResultCode: {ResultCode}", 
                    isValid, ipn.ResultCode);

                return isValid && ipn.ResultCode == 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Momo] ❌ Lỗi verify IPN");
                return false;
            }
        }

        /// <summary>
        /// Hoàn tiền qua Momo (API khác với VNPay)
        /// </summary>
        public MomoRefundResponse ProcessRefund(MomoRefundRequest request)
        {
            _logger.LogInformation("[Momo] Xử lý refund cho TransId: {TransId}, Amount: {Amount}VND", 
                request.TransId, request.Amount);

            try
            {
                // Tạo signature
                var rawSignature = $"accessKey={AccessKey}&amount={request.Amount}&description={request.Description}" +
                                  $"&orderId={request.RequestId}&partnerCode={request.PartnerCode}" +
                                  $"&requestId={request.RequestId}&transId={request.TransId}";

                request.Signature = ComputeHmacSha256(SecretKey, rawSignature);

                // Giả lập refund thành công
                _logger.LogInformation("[Momo] ✅ Refund thành công");

                return new MomoRefundResponse
                {
                    PartnerCode = request.PartnerCode,
                    RequestId = request.RequestId,
                    TransId = request.TransId,
                    ResultCode = 0,
                    Message = "Refund successful"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Momo] ❌ Lỗi refund");
                return new MomoRefundResponse
                {
                    ResultCode = 99,
                    Message = ex.Message
                };
            }
        }

        /// <summary>
        /// Tính HMAC SHA256 (Momo dùng SHA256, khác với VNPay dùng SHA512)
        /// </summary>
        private string ComputeHmacSha256(string key, string data)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
            var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }
    }
}
