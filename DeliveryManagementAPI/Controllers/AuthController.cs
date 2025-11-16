using DeliveryManagementAPI.Models;
using DeliveryManagementAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Google.Apis.Auth;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace DeliveryManagementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserAccountService _userService;
        private readonly IConfiguration _config;

        public AuthController(UserAccountService userService, IConfiguration config)
        {
            _userService = userService;
            _config = config;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest req)
        {
            var user = await _userService.AuthenticateAsync(req.Username!, req.Password!);
            if (user == null)
                return Unauthorized(new { message = "Sai tài khoản hoặc mật khẩu" });
            var token = GenerateJwtToken(user);
            return Ok(new { token, user = new { user.UserId, user.Username, user.FullName, user.Email, user.Role } });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest req)
        {
            if (await _userService.UsernameExistsAsync(req.Username!))
                return BadRequest(new { message = "Tên đăng nhập đã tồn tại" });
            if (await _userService.EmailExistsAsync(req.Email!))
                return BadRequest(new { message = "Email đã tồn tại" });
            var user = new UserAccount
            {
                Username = req.Username!,
                FullName = req.FullName!,
                Email = req.Email!,
                PhoneNumber = req.PhoneNumber!,
                Role = "customer"
            };
            await _userService.RegisterAsync(user, req.Password!);
            return Ok(new { success = true });
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest req)
        {
            var user = await _userService.GetByEmailAsync(req.Email!);
            if (user == null)
                return NotFound(new { message = "Không tìm thấy email này" });
            // Sinh token reset đơn giản (demo)
            var token = Guid.NewGuid().ToString();
            await _userService.SetResetTokenAsync(req.Email!, token, DateTime.UtcNow.AddMinutes(15));
            // TODO: Gửi email thực tế
            return Ok(new { success = true, token });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest req)
        {
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
    }

    public class LoginRequest
    {
        public string? Username { get; set; }
        public string? Password { get; set; }
    }
    public class RegisterRequest
    {
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
    }
    public class ForgotPasswordRequest
    {
        public string? Email { get; set; }
    }
    public class ResetPasswordRequest
    {
        public string? Token { get; set; }
        public string? NewPassword { get; set; }
    }

    public class GoogleSignInRequest
    {
        public string? IdToken { get; set; }
    }
}
