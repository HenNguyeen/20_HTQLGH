using System.Security.Cryptography;

namespace DeliveryManagementAPI.Services
{
    public interface ITwoFactorService
    {
        string GenerateOTP();
        Task<bool> SendOTPEmailAsync(string email, string fullName, string otp);
    }

    public class TwoFactorService : ITwoFactorService
    {
        private readonly IEmailService _emailService;

        public TwoFactorService(IEmailService emailService)
        {
            _emailService = emailService;
        }

        public string GenerateOTP()
        {
            // Generate a 6-digit OTP
            return RandomNumberGenerator.GetInt32(100000, 999999).ToString();
        }

        public async Task<bool> SendOTPEmailAsync(string email, string fullName, string otp)
        {
            var subject = "Mã xác thực đăng nhập - Delivery Management";
            var body = $@"
                <html>
                <body style='font-family: Arial, sans-serif;'>
                    <div style='max-width: 600px; margin: 0 auto; padding: 20px; background-color: #f5f5f5;'>
                        <div style='background-color: white; padding: 30px; border-radius: 10px;'>
                            <h2 style='color: #2196F3; margin-bottom: 20px;'>Xác thực đăng nhập</h2>
                            <p>Xin chào <strong>{fullName}</strong>,</p>
                            <p>Mã xác thực của bạn là:</p>
                            <div style='background-color: #f0f0f0; padding: 20px; text-align: center; font-size: 32px; font-weight: bold; letter-spacing: 5px; margin: 20px 0; border-radius: 5px;'>
                                {otp}
                            </div>
                            <p style='color: #666;'>Mã này có hiệu lực trong <strong>5 phút</strong>.</p>
                            <p style='color: #666;'>Nếu bạn không yêu cầu mã này, vui lòng bỏ qua email này.</p>
                            <hr style='margin: 30px 0; border: none; border-top: 1px solid #ddd;' />
                            <p style='font-size: 12px; color: #999;'>© 2026 Delivery Management System. All rights reserved.</p>
                        </div>
                    </div>
                </body>
                </html>
            ";

            try
            {
                await _emailService.SendEmailAsync(email, subject, body);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
