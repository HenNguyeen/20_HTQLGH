namespace DeliveryManagementAPI.Models
{
    /// <summary>
    /// DTO để nhận dữ liệu khi tạo nhân viên mới (bao gồm thông tin tài khoản)
    /// </summary>
    public class CreateStaffDto
    {
        // Thông tin nhân viên
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string VehicleType { get; set; } = string.Empty;
        public string VehiclePlate { get; set; } = string.Empty;
        public bool IsAvailable { get; set; } = true;

        // Thông tin tài khoản đăng nhập
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Password { get; set; } // Optional, nếu null sẽ dùng mật khẩu mặc định
    }

    /// <summary>
    /// DTO trả về sau khi tạo nhân viên thành công (bao gồm thông tin tài khoản)
    /// </summary>
    public class StaffAccountResponse
    {
        public DeliveryStaff Staff { get; set; } = null!;
        public UserAccount UserAccount { get; set; } = null!;
        public string PlainPassword { get; set; } = string.Empty; // Mật khẩu chưa mã hóa để hiển thị cho admin
    }
}
