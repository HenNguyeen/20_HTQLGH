using System.Security.Cryptography;

namespace DeliveryManagementAPI.Services
{
    public interface ITwoFactorService
    {
        string GenerateOTP();
        Task<bool> SendOTPEmailAsync(string email, string fullName, string otp);
        Task<bool> SendResetPasswordEmailAsync(string email, string fullName, string resetToken);
    }

    public class TwoFactorService : ITwoFactorService
    {
        private readonly IEmailService _emailService;
        private readonly IConfiguration _config;

        public TwoFactorService(IEmailService emailService, IConfiguration config)
        {
            _emailService = emailService;
            _config = config;
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

        public async Task<bool> SendResetPasswordEmailAsync(string email, string fullName, string resetToken)
        {
            var frontendUrl = _config["AppSettings:FrontendUrl"] ?? "http://localhost:3000";
            var resetLink = $"{frontendUrl}/reset-password.html?token={resetToken}";
            var subject = "Đặt lại mật khẩu - Delivery Management";
            var body = $@"
                <html>
                <body style='font-family: Arial, sans-serif;'>
                    <div style='max-width: 600px; margin: 0 auto; padding: 20px; background-color: #f5f5f5;'>
                        <div style='background-color: white; padding: 30px; border-radius: 10px;'>
                            <h2 style='color: #2196F3; margin-bottom: 20px;'>Đặt lại mật khẩu</h2>
                            <p>Xin chào <strong>{fullName}</strong>,</p>
                            <p>Chúng tôi nhận được yêu cầu đặt lại mật khẩu cho tài khoản của bạn.</p>
                            <p>Nhấn vào nút bên dưới để đặt lại mật khẩu:</p>
                            <div style='margin: 30px 0; text-align: center;'>
                                <a href='{resetLink}' style='background-color: #2196F3; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; font-weight: bold;'>Đặt lại mật khẩu</a>
                            </div>
                            <p style='color: #666; font-size: 14px;'>Hoặc copy link này vào trình duyệt:</p>
                            <p style='color: #666; font-size: 12px; word-break: break-all;'>{resetLink}</p>
                            <p style='color: #666;'>Liên kết này có hiệu lực trong <strong>15 phút</strong>.</p>
                            <p style='color: #666;'>Nếu bạn không yêu cầu đặt lại mật khẩu, vui lòng bỏ qua email này.</p>
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
