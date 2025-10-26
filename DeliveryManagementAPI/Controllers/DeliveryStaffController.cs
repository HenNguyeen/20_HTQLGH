using Microsoft.AspNetCore.Mvc;
using DeliveryManagementAPI.Models;
using DeliveryManagementAPI.Services;
using Microsoft.AspNetCore.Authorization;

namespace DeliveryManagementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Yêu cầu authentication
    public class DeliveryStaffController : ControllerBase
    {
        private readonly DeliveryStaffService _staffService;
        private readonly UserAccountService _userService;
        private readonly ILogger<DeliveryStaffController> _logger;

        public DeliveryStaffController(
            DeliveryStaffService staffService,
            UserAccountService userService,
            ILogger<DeliveryStaffController> logger)
        {
            _staffService = staffService;
            _userService = userService;
            _logger = logger;
        }

        /// <summary>
        /// Lấy danh sách tất cả nhân viên giao hàng
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<DeliveryStaff>>> GetAllStaff()
        {
            try
            {
                var staff = await _staffService.GetAllStaffAsync();
                return Ok(staff);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all delivery staff");
                return StatusCode(500, "Lỗi khi lấy danh sách nhân viên");
            }
        }

        /// <summary>
        /// Lấy thông tin nhân viên theo ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<DeliveryStaff>> GetStaffById(int id)
        {
            try
            {
                var staff = await _staffService.GetStaffByIdAsync(id);
                
                if (staff == null)
                {
                    return NotFound($"Không tìm thấy nhân viên với ID: {id}");
                }

                return Ok(staff);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting staff by ID: {StaffId}", id);
                return StatusCode(500, "Lỗi khi lấy thông tin nhân viên");
            }
        }

        /// <summary>
        /// Lấy danh sách nhân viên đang rảnh
        /// </summary>
        [HttpGet("available")]
        public async Task<ActionResult<List<DeliveryStaff>>> GetAvailableStaff()
        {
            try
            {
                var staff = await _staffService.GetAvailableStaffAsync();
                return Ok(staff);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available staff");
                return StatusCode(500, "Lỗi khi lấy danh sách nhân viên rảnh");
            }
        }

        /// <summary>
        /// Lấy thông tin nhân viên gắn với tài khoản đăng nhập hiện tại
        /// </summary>
        [HttpGet("me")]
        [Authorize(Roles = "shipper,admin")] // admin có thể test
        public async Task<ActionResult<DeliveryStaff>> GetMyStaffRecord()
        {
            try
            {
                var fullName = User.Claims.FirstOrDefault(c => c.Type == "FullName")?.Value
                               ?? User.Claims.FirstOrDefault(c => c.Type.Contains("/name"))?.Value;
                var email = User.Claims.FirstOrDefault(c => c.Type.Contains("/email"))?.Value;
                var phone = User.Claims.FirstOrDefault(c => c.Type.Contains("phone"))?.Value; // tuỳ claim

                DeliveryStaff? staff = null;
                if (!string.IsNullOrEmpty(phone))
                {
                    staff = await _staffService.GetByPhoneAsync(phone);
                }
                if (staff == null && !string.IsNullOrEmpty(fullName))
                {
                    staff = await _staffService.GetByFullNameAsync(fullName);
                }

                if (staff == null)
                {
                    return NotFound(new { message = "Không tìm thấy nhân viên tương ứng với tài khoản hiện tại" });
                }

                return Ok(staff);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current staff record");
                return StatusCode(500, new { message = "Lỗi khi lấy thông tin nhân viên hiện tại" });
            }
        }

        /// <summary>
        /// Thêm nhân viên giao hàng mới (kèm tạo tài khoản đăng nhập)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "admin")] // Chỉ admin được thêm nhân viên
        public async Task<ActionResult<StaffAccountResponse>> CreateStaff([FromBody] CreateStaffDto dto)
        {
            try
            {
                // Kiểm tra username đã tồn tại chưa
                var existingUser = await _userService.GetByUsernameAsync(dto.Username);
                if (existingUser != null)
                {
                    return BadRequest(new { message = "Tên đăng nhập đã tồn tại" });
                }

                // Kiểm tra email đã tồn tại chưa
                var existingEmail = await _userService.GetByEmailAsync(dto.Email);
                if (existingEmail != null)
                {
                    return BadRequest(new { message = "Email đã tồn tại" });
                }

                // Tạo nhân viên giao hàng
                var staff = new DeliveryStaff
                {
                    FullName = dto.FullName,
                    PhoneNumber = dto.PhoneNumber,
                    VehicleType = dto.VehicleType,
                    VehiclePlate = dto.VehiclePlate,
                    IsAvailable = dto.IsAvailable
                };

                var createdStaff = await _staffService.AddStaffAsync(staff);

                // Tạo tài khoản đăng nhập cho shipper
                var plainPassword = dto.Password ?? "123456"; // Mật khẩu mặc định
                var userAccount = new UserAccount
                {
                    Username = dto.Username,
                    PasswordHash = UserAccountService.HashPassword(plainPassword),
                    FullName = dto.FullName,
                    Email = dto.Email,
                    PhoneNumber = dto.PhoneNumber,
                    Role = "shipper"
                };

                var createdUser = await _userService.CreateUserAsync(userAccount);

                // Trả về thông tin nhân viên và tài khoản
                var response = new StaffAccountResponse
                {
                    Staff = createdStaff,
                    UserAccount = new UserAccount
                    {
                        UserId = createdUser.UserId,
                        Username = createdUser.Username,
                        FullName = createdUser.FullName,
                        Email = createdUser.Email,
                        PhoneNumber = createdUser.PhoneNumber,
                        Role = createdUser.Role
                    },
                    PlainPassword = plainPassword // Trả về mật khẩu chưa mã hóa để admin có thể cung cấp cho shipper
                };

                return CreatedAtAction(
                    nameof(GetStaffById), 
                    new { id = createdStaff.StaffId }, 
                    response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating delivery staff");
                return StatusCode(500, new { message = "Lỗi khi tạo nhân viên" });
            }
        }

        /// <summary>
        /// Cập nhật nhân viên
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "admin")] // Chỉ admin được cập nhật nhân viên
        public async Task<ActionResult<DeliveryStaff>> UpdateStaff(int id, [FromBody] DeliveryStaff staff)
        {
            try
            {
                if (id != staff.StaffId)
                {
                    return BadRequest("ID không khớp");
                }

                var updatedStaff = await _staffService.UpdateStaffAsync(staff);
                
                if (updatedStaff == null)
                {
                    return NotFound($"Không tìm thấy nhân viên với ID: {id}");
                }

                return Ok(updatedStaff);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating staff: {StaffId}", id);
                return StatusCode(500, "Lỗi khi cập nhật nhân viên");
            }
        }

        /// <summary>
        /// Cập nhật trạng thái sẵn sàng của nhân viên
        /// </summary>
        [HttpPatch("{id}/availability")]
        [Authorize(Roles = "admin,shipper")] // Admin và shipper được cập nhật trạng thái
        public async Task<ActionResult<DeliveryStaff>> UpdateAvailability(
            int id, 
            [FromBody] bool isAvailable)
        {
            try
            {
                var success = await _staffService.UpdateAvailabilityAsync(id, isAvailable);
                
                if (!success)
                {
                    return NotFound($"Không tìm thấy nhân viên với ID: {id}");
                }

                var staff = await _staffService.GetStaffByIdAsync(id);
                return Ok(staff);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating staff availability: {StaffId}", id);
                return StatusCode(500, "Lỗi khi cập nhật trạng thái nhân viên");
            }
        }

        /// <summary>
        /// Xóa nhân viên
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")] // Chỉ admin được xóa nhân viên
        public async Task<ActionResult> DeleteStaff(int id)
        {
            try
            {
                var success = await _staffService.DeleteStaffAsync(id);
                
                if (!success)
                {
                    return NotFound($"Không tìm thấy nhân viên với ID: {id}");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting staff: {StaffId}", id);
                return StatusCode(500, "Lỗi khi xóa nhân viên");
            }
        }
    }
}
