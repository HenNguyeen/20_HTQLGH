using DeliveryManagementAPI.Models;
using DeliveryManagementAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DeliveryManagementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProfileController : ControllerBase
    {
        private readonly UserAccountService _userService;

        public ProfileController(UserAccountService userService)
        {
            _userService = userService;
        }

        private int? GetCurrentUserId()
        {
            var idClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(idClaim, out var id)) return id;
            return null;
        }

        [HttpGet("me")]
        public async Task<ActionResult<object>> GetMe()
        {
            var id = GetCurrentUserId();
            if (id == null) return Unauthorized();
            var user = await _userService.GetByIdAsync(id.Value);
            if (user == null) return NotFound();
            return Ok(new
            {
                user.UserId,
                user.Username,
                user.FullName,
                user.Email,
                user.PhoneNumber,
                user.Role
            });
        }

        public class UpdateMeDto
        {
            public string? FullName { get; set; }
            public string? Email { get; set; }
            public string? PhoneNumber { get; set; }
        }

        [HttpPut("me")]
        public async Task<ActionResult<object>> UpdateMe([FromBody] UpdateMeDto dto)
        {
            var id = GetCurrentUserId();
            if (id == null) return Unauthorized();
            var user = await _userService.GetByIdAsync(id.Value);
            if (user == null) return NotFound();

            // If email changed, ensure unique
            if (!string.IsNullOrWhiteSpace(dto.Email) && dto.Email != user.Email)
            {
                var exists = await _userService.EmailExistsAsync(dto.Email);
                if (exists) return BadRequest(new { message = "Email đã tồn tại" });
            }

            user.FullName = dto.FullName ?? user.FullName;
            user.Email = dto.Email ?? user.Email;
            user.PhoneNumber = dto.PhoneNumber ?? user.PhoneNumber;
            await _userService.UpdateAsync(user);

            return Ok(new
            {
                user.UserId,
                user.Username,
                user.FullName,
                user.Email,
                user.PhoneNumber,
                user.Role
            });
        }

        public class ChangePasswordDto
        {
            public string? CurrentPassword { get; set; }
            public string? NewPassword { get; set; }
        }

        [HttpPatch("change-password")]
        public async Task<ActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            var id = GetCurrentUserId();
            if (id == null) return Unauthorized();
            var user = await _userService.GetByIdAsync(id.Value);
            if (user == null) return NotFound();
            if (string.IsNullOrWhiteSpace(dto?.NewPassword)) return BadRequest(new { message = "Mật khẩu mới không được trống" });

            // If current password is provided, verify (optional requirement)
            if (!string.IsNullOrEmpty(dto!.CurrentPassword))
            {
                if (!_userService.CheckPassword(user, dto.CurrentPassword))
                    return BadRequest(new { message = "Mật khẩu hiện tại không đúng" });
            }

            await _userService.ResetPasswordAsync(user, dto.NewPassword!);
            return Ok(new { success = true });
        }
    }
}
