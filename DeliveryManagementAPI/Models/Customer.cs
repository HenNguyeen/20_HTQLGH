namespace DeliveryManagementAPI.Models
{
    /// <summary>
    /// Thông tin khách hàng
    /// </summary>
    public class Customer
    {
        [System.ComponentModel.DataAnnotations.Key]
        public int CustomerId { get; set; }
        
        // 1. Thông tin định danh (Bắt buộc)
        public string FullName { get; set; } = string.Empty;      // Họ tên / Tên Shop
        public string PhoneNumber { get; set; } = string.Empty;   // Số điện thoại (tên đăng nhập)
        public string Email { get; set; } = string.Empty;         // Email
        
        // 2. Thông tin địa chỉ lấy hàng (Pickup Address)
        public string Address { get; set; } = string.Empty;       // Địa chỉ chi tiết (Số nhà, tên đường)
        public string Ward { get; set; } = string.Empty;          // Phường/Xã
        public string District { get; set; } = string.Empty;      // Quận/Huyện
        public string City { get; set; } = string.Empty;          // Thành phố
        public string AddressType { get; set; } = "Kho hàng";      // Loại địa chỉ: Kho hàng, Nhà riêng, Văn phòng
        
        // 3. Thông tin Tài chính & Đối soát (Dành cho Shop)
        // Thông tin ngân hàng
        public string? BankAccountNumber { get; set; }            // Số tài khoản
        public string? BankAccountName { get; set; }              // Tên chủ tài khoản
        public string? BankName { get; set; }                     // Tên ngân hàng
        public string? BankBranch { get; set; }                   // Chi nhánh ngân hàng
        
        // Chu kỳ đối soát
        public string? SettlementCycle { get; set; }              // Daily (Hàng ngày), Weekly (Hàng tuần), Monthly (Hàng tháng), OnDemand (Theo yêu cầu), MinimumBalance (Khi đạt số dư tối thiểu)
        
        // Thông tin khác
        public string? TaxCode { get; set; }                      // Mã số thuế
        public DateTime CreatedDate { get; set; } = DateTime.Now;  // Ngày tạo
        public DateTime? UpdatedDate { get; set; }                 // Ngày cập nhật lần cuối
    }
}
