using DeliveryManagementAPI.Models;
using DeliveryManagementAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Google.Apis.Auth;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace DeliveryManagementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserAccountService _userService;
        private readonly IConfiguration _config;
        private readonly ITwoFactorService _twoFactorService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            UserAccountService userService, 
            IConfiguration config,
            ITwoFactorService twoFactorService,
            ILogger<AuthController> logger)
        {
            _userService = userService;
            _config = config;
            _twoFactorService = twoFactorService;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest req)
        {
            var user = await _userService.AuthenticateAsync(req.Username!, req.Password!);
            if (user == null)
                return Unauthorized(new { message = "Sai tài khoản hoặc mật khẩu" });

            // Check if 2FA is enabled for this user
            if (user.TwoFactorEnabled)
            {
                // Generate and send OTP
                var otp = _twoFactorService.GenerateOTP();
                await _userService.SetTwoFactorCodeAsync(user.UserId, otp, DateTime.UtcNow.AddMinutes(5));
                
                // Send OTP via email
                await _twoFactorService.SendOTPEmailAsync(user.Email, user.FullName, otp);

                return Ok(new 
                { 
                    requiresTwoFactor = true, 
                    userId = user.UserId,
                    message = "Mã xác thực đã được gửi đến email của bạn"
                });
            }

            var token = GenerateJwtToken(user);
            return Ok(new { token, user = new { user.UserId, user.Username, user.FullName, user.Email, user.Role } });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest req)
        {
            // Normalize and sanitize inputs
            var fullName = ValidationHelper.NormalizeInput(req.FullName);
            var email = ValidationHelper.NormalizeInput(req.Email)?.ToLower();
            var phoneNumber = ValidationHelper.NormalizeInput(req.PhoneNumber);
            var username = ValidationHelper.NormalizeInput(req.Username)?.ToLower();
            
            // Validate Full Name
            var fullNameValidation = ValidationHelper.ValidateFullName(fullName);
            if (!fullNameValidation.IsValid)
                return BadRequest(new { message = fullNameValidation.ErrorMessage, field = "fullName" });
            
            // Validate Email
            var emailValidation = ValidationHelper.ValidateEmail(email);
            if (!emailValidation.IsValid)
                return BadRequest(new { message = emailValidation.ErrorMessage, field = "email" });
            
            // Validate Phone Number
            var phoneValidation = ValidationHelper.ValidatePhoneNumber(phoneNumber);
            if (!phoneValidation.IsValid)
                return BadRequest(new { message = phoneValidation.ErrorMessage, field = "phoneNumber" });
            
            // Validate Username
            var usernameValidation = ValidationHelper.ValidateUsername(username);
            if (!usernameValidation.IsValid)
                return BadRequest(new { message = usernameValidation.ErrorMessage, field = "username" });
            
            // Validate Password
            var passwordValidation = ValidationHelper.ValidatePassword(req.Password);
            if (!passwordValidation.IsValid)
                return BadRequest(new { message = passwordValidation.ErrorMessage, field = "password" });
            
            // Validate Confirm Password
            var confirmPasswordValidation = ValidationHelper.ValidateConfirmPassword(req.Password, req.ConfirmPassword);
            if (!confirmPasswordValidation.IsValid)
                return BadRequest(new { message = confirmPasswordValidation.ErrorMessage, field = "confirmPassword" });
            
            // Validate Terms Acceptance
            var termsValidation = ValidationHelper.ValidateTermsCheckbox(req.AcceptTerms);
            if (!termsValidation.IsValid)
                return BadRequest(new { message = termsValidation.ErrorMessage, field = "acceptTerms" });
            
            // Check if username already exists
            if (await _userService.UsernameExistsAsync(username!))
                return BadRequest(new { message = "Tên đăng nhập đã tồn tại", field = "username" });
            
            // Check if email already exists
            if (await _userService.EmailExistsAsync(email!))
                return BadRequest(new { message = "Email đã tồn tại", field = "email" });
            
            try
            {
                var user = new UserAccount
                {
                    Username = username!,
                    FullName = fullName,
                    Email = email!,
                    PhoneNumber = phoneNumber!,
                    Role = "customer"
                };
                
                await _userService.RegisterAsync(user, req.Password!);
                return Ok(new { success = true, message = "Đăng ký thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration");
                return StatusCode(500, new { message = "Lỗi khi đăng ký. Vui lòng thử lại sau." });
            }
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest req)
        {
            var user = await _userService.GetByEmailAsync(req.Email!);
            if (user == null)
                return NotFound(new { message = "Không tìm thấy email này" });
            
            // Generate reset token
            var token = Guid.NewGuid().ToString();
            await _userService.SetResetTokenAsync(req.Email!, token, DateTime.UtcNow.AddMinutes(15));
            
            // Send reset link via email
            await _twoFactorService.SendResetPasswordEmailAsync(user.Email, user.FullName, token);
            
            return Ok(new { success = true, message = "Email đặt lại mật khẩu đã được gửi. Vui lòng kiểm tra email của bạn." });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest req)
        {
            // Validate password strength
            var passwordValidation = ValidationHelper.ValidatePassword(req.NewPassword);
            if (!passwordValidation.IsValid)
                return BadRequest(new { message = passwordValidation.ErrorMessage });
            
            var user = await _userService.GetByResetTokenAsync(req.Token!);
            if (user == null)
                return BadRequest(new { message = "Token không hợp lệ hoặc đã hết hạn" });
            
            await _userService.ResetPasswordAsync(user, req.NewPassword!);
            return Ok(new { success = true });
        }

        [HttpPost("google")]
        [AllowAnonymous]
        public async Task<IActionResult> GoogleSignIn([FromBody] GoogleSignInRequest req)
        {
            if (string.IsNullOrEmpty(req.IdToken))
                return BadRequest(new { message = "Id token required" });

            try
            {
                var settings = new GoogleJsonWebSignature.ValidationSettings()
                {
                    Audience = new[] { _config["Authentication:Google:ClientId"] }
                };

                var payload = await GoogleJsonWebSignature.ValidateAsync(req.IdToken, settings);

                var googleId = payload.Subject; // sub
                var email = payload.Email;

                // find existing user by googleId or email
                var user = await _userService.GetByEmailAsync(email!);
                if (user == null || user.GoogleId != googleId)
                {
                    // if user exists but no GoogleId, link it
                    if (user != null && string.IsNullOrEmpty(user.GoogleId))
                    {
                        user.GoogleId = googleId;
                        await _userService.UpdateAsync(user);
                    }
                    else if (user == null)
                    {
                        // create new user (customer)
                        var newUser = new UserAccount
                        {
                            Username = email!,
                            Email = email!,
                            FullName = payload.Name ?? string.Empty,
                            Role = "customer",
                            GoogleId = googleId
                        };
                        user = await _userService.CreateUserAsync(newUser);
                    }
                }

                var token = GenerateJwtToken(user);
                return Ok(new { token, user = new { user.UserId, user.Username, user.FullName, user.Email, user.Role } });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Invalid Google token", detail = ex.Message });
            }
        }

        private string GenerateJwtToken(UserAccount user)
        {
            var jwtSettings = _config.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"] ?? "YourSuperSecretKeyForJWT12345678901234567890";
            var expiryMinutes = int.Parse(jwtSettings["ExpiryMinutes"] ?? "60");

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("FullName", user.FullName)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"] ?? "DeliveryManagementAPI",
                audience: jwtSettings["Audience"] ?? "DeliveryManagementClients",
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        [HttpPost("verify-2fa")]
        public async Task<IActionResult> VerifyTwoFactor([FromBody] VerifyTwoFactorRequest req)
        {
            var isValid = await _userService.VerifyTwoFactorCodeAsync(req.UserId, req.Code!);
            if (!isValid)
                return BadRequest(new { message = "Mã xác thực không đúng hoặc đã hết hạn" });

            var user = await _userService.GetByIdAsync(req.UserId);
            if (user == null)
                return NotFound(new { message = "Không tìm thấy người dùng" });

            var token = GenerateJwtToken(user);
            return Ok(new { token, user = new { user.UserId, user.Username, user.FullName, user.Email, user.Role } });
        }

        [HttpPost("enable-2fa")]
        [Authorize]
        public async Task<IActionResult> EnableTwoFactor()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized();

            var userId = int.Parse(userIdClaim);
            var success = await _userService.EnableTwoFactorAsync(userId);
            
            if (!success)
                return NotFound(new { message = "Không tìm thấy người dùng" });

            return Ok(new { success = true, message = "Đã bật xác thực 2 yếu tố" });
        }

        [HttpPost("disable-2fa")]
        [Authorize]
        public async Task<IActionResult> DisableTwoFactor()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized();

            var userId = int.Parse(userIdClaim);
            var success = await _userService.DisableTwoFactorAsync(userId);
            
            if (!success)
                return NotFound(new { message = "Không tìm thấy người dùng" });

            return Ok(new { success = true, message = "Đã tắt xác thực 2 yếu tố" });
        }

        [HttpPost("resend-2fa")]
        public async Task<IActionResult> ResendTwoFactorCode([FromBody] ResendTwoFactorRequest req)
        {
            var user = await _userService.GetByIdAsync(req.UserId);
            if (user == null)
                return NotFound(new { message = "Không tìm thấy người dùng" });

            if (!user.TwoFactorEnabled)
                return BadRequest(new { message = "Xác thực 2 yếu tố chưa được bật" });

            // Generate and send new OTP
            var otp = _twoFactorService.GenerateOTP();
            await _userService.SetTwoFactorCodeAsync(user.UserId, otp, DateTime.UtcNow.AddMinutes(5));
            await _twoFactorService.SendOTPEmailAsync(user.Email, user.FullName, otp);

            return Ok(new { success = true, message = "Mã xác thực mới đã được gửi" });
        }
    }

    public class LoginRequest
    {
        [Required(ErrorMessage = "Tên đăng nhập không được để trống")]
        public string? Username { get; set; }
        
        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        public string? Password { get; set; }
    }
    
    public class RegisterRequest
    {
        /// <summary>
        /// Họ và tên (2-100 ký tự, chỉ chứa chữ cái và khoảng trắng)
        /// </summary>
        [Required(ErrorMessage = "Họ và tên không được để trống")]
        [MinLength(2, ErrorMessage = "Họ và tên phải có ít nhất 2 ký tự")]
        [MaxLength(100, ErrorMessage = "Họ và tên không được vượt quá 100 ký tự")]
        [RegularExpression(@"^[a-zA-Z\s\u0100-\u0177\u01A0-\u01A1\u1EA0-\u1EFF]+$", 
            ErrorMessage = "Họ và tên chỉ được chứa chữ cái và khoảng trắng")]
        public string? FullName { get; set; }
        
        /// <summary>
        /// Email (không chứa khoảng trắng, max 255 ký tự)
        /// </summary>
        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Định dạng email không hợp lệ (ví dụ: example@gmail.com)")]
        [MaxLength(255, ErrorMessage = "Email không được vượt quá 255 ký tự")]
        public string? Email { get; set; }
        
        /// <summary>
        /// Số điện thoại (10-11 chữ số, bắt đầu với 0)
        /// </summary>
        [Required(ErrorMessage = "Số điện thoại không được để trống")]
        [RegularExpression(@"^0\d{9,10}$", 
            ErrorMessage = "Số điện thoại phải bắt đầu với 0 và có 10-11 chữ số")]
        public string? PhoneNumber { get; set; }
        
        /// <summary>
        /// Tên đăng nhập (5-50 ký tự, chỉ chứa chữ cái và số)
        /// </summary>
        [Required(ErrorMessage = "Tên đăng nhập không được để trống")]
        [MinLength(5, ErrorMessage = "Tên đăng nhập phải có ít nhất 5 ký tự")]
        [MaxLength(50, ErrorMessage = "Tên đăng nhập không được vượt quá 50 ký tự")]
        [RegularExpression(@"^[a-zA-Z0-9]+$", 
            ErrorMessage = "Tên đăng nhập chỉ được chứa chữ cái và số")]
        public string? Username { get; set; }
        
        /// <summary>
        /// Mật khẩu (8+ ký tự, chứa chữ hoa, chữ thường, số, ký tự đặc biệt)
        /// </summary>
        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        [MinLength(8, ErrorMessage = "Mật khẩu phải có ít nhất 8 ký tự")]
        [RegularExpression(@"^(?=.*[A-Z])(?=.*[a-z])(?=.*[0-9])(?=.*[!@#$%^&*]).*$", 
            ErrorMessage = "Mật khẩu phải chứa ít nhất 1 chữ hoa, 1 chữ thường, 1 số, 1 ký tự đặc biệt (!@#$%^&*)")]
        public string? Password { get; set; }
        
        /// <summary>
        /// Xác nhận mật khẩu (phải khớp với Password)
        /// </summary>
        [Required(ErrorMessage = "Xác nhận mật khẩu không được để trống")]
        [Compare("Password", ErrorMessage = "Xác nhận mật khẩu không khớp")]
        public string? ConfirmPassword { get; set; }
        
        /// <summary>
        /// Đồng ý với Điều khoản dịch vụ
        /// </summary>
        [Required(ErrorMessage = "Bạn phải đồng ý với Điều khoản dịch vụ")]
        public bool AcceptTerms { get; set; }
    }
    public class ForgotPasswordRequest
    {
        public string? Email { get; set; }
    }
    public class ResetPasswordRequest
    {
        [Required(ErrorMessage = "Token không được để trống")]
        public string? Token { get; set; }
        
        [Required(ErrorMessage = "Mật khẩu mới không được để trống")]
        [MinLength(8, ErrorMessage = "Mật khẩu phải có ít nhất 8 ký tự")]
        public string? NewPassword { get; set; }
    }

    public class GoogleSignInRequest
    {
        public string? IdToken { get; set; }
    }

    public class VerifyTwoFactorRequest
    {
        public int UserId { get; set; }
        public string? Code { get; set; }
    }

    public class ResendTwoFactorRequest
    {
        public int UserId { get; set; }
    }
}
