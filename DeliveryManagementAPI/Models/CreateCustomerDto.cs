namespace DeliveryManagementAPI.Models
{
    /// <summary>
    /// DTO để tạo khách hàng mới
    /// </summary>
    public class CreateCustomerDto
    {
        // 1. Thông tin định danh (Bắt buộc)
        public string FullName { get; set; } = string.Empty;      // Họ tên / Tên Shop
        public string PhoneNumber { get; set; } = string.Empty;   // Số điện thoại
        public string Email { get; set; } = string.Empty;         // Email

        // 2. Thông tin địa chỉ lấy hàng (Pickup Address)
        public string Address { get; set; } = string.Empty;       // Địa chỉ chi tiết
        public string Ward { get; set; } = string.Empty;          // Phường/Xã
        public string District { get; set; } = string.Empty;      // Quận/Huyện
        public string City { get; set; } = string.Empty;          // Thành phố
        public string AddressType { get; set; } = "Kho hàng";      // Loại địa chỉ: Kho hàng, Nhà riêng, Văn phòng

        // 3. Thông tin Tài chính & Đối soát (Dành cho Shop)
        public string? BankAccountNumber { get; set; }            // Số tài khoản
        public string? BankAccountName { get; set; }              // Tên chủ tài khoản
        public string? BankName { get; set; }                     // Tên ngân hàng
        public string? BankBranch { get; set; }                   // Chi nhánh ngân hàng
        public string? SettlementCycle { get; set; }              // Chu kỳ đối soát
        public string? TaxCode { get; set; }                      // Mã số thuế
    }

    /// <summary>
    /// DTO để cập nhật thông tin khách hàng
    /// </summary>
    public class UpdateCustomerDto
    {
        public int CustomerId { get; set; }

        // 1. Thông tin định danh
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        // 2. Thông tin địa chỉ lấy hàng
        public string Address { get; set; } = string.Empty;
        public string Ward { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string AddressType { get; set; } = "Kho hàng";

        // 3. Thông tin Tài chính & Đối soát
        public string? BankAccountNumber { get; set; }
        public string? BankAccountName { get; set; }
        public string? BankName { get; set; }
        public string? BankBranch { get; set; }
        public string? SettlementCycle { get; set; }
        public string? TaxCode { get; set; }
    }

    /// <summary>
    /// DTO trả về thông tin khách hàng
    /// </summary>
    public class CustomerResponse
    {
        public int CustomerId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Ward { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string AddressType { get; set; } = string.Empty;
        public string? BankAccountNumber { get; set; }
        public string? BankAccountName { get; set; }
        public string? BankName { get; set; }
        public string? BankBranch { get; set; }
        public string? SettlementCycle { get; set; }
        public string? TaxCode { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}
