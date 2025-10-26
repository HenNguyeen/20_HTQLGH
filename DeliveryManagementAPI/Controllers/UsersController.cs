using DeliveryManagementAPI.Models;
using DeliveryManagementAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeliveryManagementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "admin")]
    public class UsersController : ControllerBase
    {
        private readonly UserAccountService _userService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(UserAccountService userService, ILogger<UsersController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<List<UserAccount>>> GetAll()
        {
            var users = await _userService.GetAllAsync();
            return Ok(users.Select(u => new UserAccount
            {
                UserId = u.UserId,
                Username = u.Username,
                FullName = u.FullName,
                Email = u.Email,
                PhoneNumber = u.PhoneNumber,
                Role = u.Role
            }).ToList());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserAccount>> GetById(int id)
        {
            var user = await _userService.GetByIdAsync(id);
            if (user == null) return NotFound();
            return Ok(new UserAccount
            {
                UserId = user.UserId,
                Username = user.Username,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Role = user.Role
            });
        }

        [HttpPost]
        public async Task<ActionResult<UserAccount>> Create([FromBody] CreateUserDto dto)
        {
            try
            {
                if (await _userService.UsernameExistsAsync(dto.Username!))
                {
                    return BadRequest(new { message = "Tên đăng nhập đã tồn tại" });
                }
                if (!string.IsNullOrWhiteSpace(dto.Email) && await _userService.EmailExistsAsync(dto.Email!))
                {
                    return BadRequest(new { message = "Email đã tồn tại" });
                }

                var user = new UserAccount
                {
                    Username = dto.Username!,
                    FullName = dto.FullName ?? string.Empty,
                    Email = dto.Email ?? string.Empty,
                    PhoneNumber = dto.PhoneNumber ?? string.Empty,
                    Role = string.IsNullOrWhiteSpace(dto.Role) ? "customer" : dto.Role!
                };

                var created = await _userService.RegisterAsync(user, dto.Password ?? "123456");
                return CreatedAtAction(nameof(GetById), new { id = created.UserId }, new UserAccount
                {
                    UserId = created.UserId,
                    Username = created.Username,
                    FullName = created.FullName,
                    Email = created.Email,
                    PhoneNumber = created.PhoneNumber,
                    Role = created.Role
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user");
                return StatusCode(500, new { message = "Lỗi khi tạo tài khoản" });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<UserAccount>> Update(int id, [FromBody] UpdateUserDto dto)
        {
            var user = await _userService.GetByIdAsync(id);
            if (user == null) return NotFound();

            if (!string.IsNullOrWhiteSpace(dto.Email) && dto.Email != user.Email && await _userService.EmailExistsAsync(dto.Email))
            {
                return BadRequest(new { message = "Email đã tồn tại" });
            }

            // Update fields
            user.FullName = dto.FullName ?? user.FullName;
            user.Email = dto.Email ?? user.Email;
            user.PhoneNumber = dto.PhoneNumber ?? user.PhoneNumber;
            if (!string.IsNullOrWhiteSpace(dto.Role))
            {
                user.Role = dto.Role!;
            }

            await _userService.UpdateAsync(user);

            return Ok(new UserAccount
            {
                UserId = user.UserId,
                Username = user.Username,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Role = user.Role
            });
        }

        [HttpPatch("{id}/password")]
        public async Task<ActionResult> ResetPassword(int id, [FromBody] ResetUserPasswordDto dto)
        {
            var user = await _userService.GetByIdAsync(id);
            if (user == null) return NotFound();
            var newPassword = string.IsNullOrWhiteSpace(dto.NewPassword) ? "123456" : dto.NewPassword!;
            await _userService.ResetPasswordAsync(user, newPassword);
            return Ok(new { success = true });
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var deleted = await _userService.DeleteAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }
    }

    public class CreateUserDto
    {
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Role { get; set; }
    }

    public class UpdateUserDto
    {
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Role { get; set; }
    }

    public class ResetUserPasswordDto
    {
        public string? NewPassword { get; set; }
    }
}
