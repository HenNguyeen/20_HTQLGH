using System.ComponentModel.DataAnnotations;

namespace DeliveryManagementAPI.Models
{
    /// <summary>
    /// DTO để nhận dữ liệu khi tạo đơn hàng mới
    /// Bao gồm validation cho tất cả các trường
    /// </summary>
    public class CreateOrderDto
    {
        /// <summary>
        /// Mã đơn hàng (system-generated hoặc DH + timestamp)
        /// Max: 50 ký tự, chỉ chứa chữ, số, dấu gạch dưới
        /// </summary>
        [Required(ErrorMessage = "Mã đơn hàng không được để trống")]
        [MaxLength(50, ErrorMessage = "Mã đơn hàng không được vượt quá 50 ký tự")]
        [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "Mã đơn hàng chỉ được chứa chữ cái, số và dấu gạch dưới")]
        public string OrderCode { get; set; } = string.Empty;
        
        // ========== THÔNG TIN KHÁCH HÀNG ==========
        
        /// <summary>
        /// Tên khách hàng
        /// 2-100 ký tự, chỉ chứa chữ cái và khoảng trắng
        /// </summary>
        [Required(ErrorMessage = "Tên khách hàng không được để trống")]
        [MinLength(2, ErrorMessage = "Tên khách hàng phải có ít nhất 2 ký tự")]
        [MaxLength(100, ErrorMessage = "Tên khách hàng không được vượt quá 100 ký tự")]
        [RegularExpression(@"^[a-zA-Z\s\u0100-\u0177\u01A0-\u01A1\u1EA0-\u1EFF]+$", 
            ErrorMessage = "Tên khách hàng chỉ được chứa chữ cái và khoảng trắng")]
        public string CustomerName { get; set; } = string.Empty;

        /// <summary>
        /// Số điện thoại khách hàng
        /// 10-11 chữ số, bắt đầu với 0
        /// </summary>
        [Required(ErrorMessage = "Số điện thoại không được để trống")]
        [RegularExpression(@"^0\d{9,10}$", 
            ErrorMessage = "Số điện thoại phải bắt đầu với 0 và có 10-11 chữ số")]
        public string CustomerPhone { get; set; } = string.Empty;

        /// <summary>
        /// Địa chỉ giao hàng
        /// 5-255 ký tự
        /// </summary>
        [Required(ErrorMessage = "Địa chỉ giao hàng không được để trống")]
        [MinLength(5, ErrorMessage = "Địa chỉ giao hàng phải ít nhất 5 ký tự")]
        [MaxLength(255, ErrorMessage = "Địa chỉ giao hàng không được vượt quá 255 ký tự")]
        public string DeliveryAddress { get; set; } = string.Empty;

        /// <summary>
        /// Phường/Xã
        /// </summary>
        [Required(ErrorMessage = "Phường/Xã không được để trống")]
        [MinLength(1, ErrorMessage = "Phường/Xã không được để trống")]
        public string Ward { get; set; } = string.Empty;

        /// <summary>
        /// Quận/Huyện
        /// </summary>
        [Required(ErrorMessage = "Quận/Huyện không được để trống")]
        [MinLength(1, ErrorMessage = "Quận/Huyện không được để trống")]
        public string District { get; set; } = string.Empty;

        /// <summary>
        /// Thành Phố
        /// </summary>
        [Required(ErrorMessage = "Thành Phố không được để trống")]
        [MinLength(1, ErrorMessage = "Thành Phố không được để trống")]
        public string City { get; set; } = string.Empty;
        
        // ========== THÔNG TIN HÀNG HÓA ==========
        
        /// <summary>
        /// Mã sản phẩm
        /// Chỉ chứa chữ, số, dấu gạch dưới
        /// </summary>
        [Required(ErrorMessage = "Mã sản phẩm không được để trống")]
        [RegularExpression(@"^[a-zA-Z0-9_]+$", 
            ErrorMessage = "Mã sản phẩm chỉ được chứa chữ cái, số và dấu gạch dưới")]
        public string ProductCode { get; set; } = string.Empty;

        /// <summary>
        /// Loại hàng
        /// Options: "Gói nhỏ", "Gói lớn", "Gói chuẩn", "Khác"
        /// </summary>
        public PackageType PackageType { get; set; }

        /// <summary>
        /// Trọng lượng (kg)
        /// Phải > 0 và <= 1000
        /// </summary>
        [Required(ErrorMessage = "Trọng lượng không được để trống")]
        [Range(0.01, 1000, ErrorMessage = "Trọng lượng phải từ 0.01 đến 1000 kg")]
        public double Weight { get; set; }

        /// <summary>
        /// Kích thước (LxWxH cm) - stored as string e.g., "10x10x10"
        /// </summary>
        [Required(ErrorMessage = "Kích thước không được để trống")]
        [MinLength(5, ErrorMessage = "Kích thước định dạng không hợp lệ")]
        public string Size { get; set; } = string.Empty;

        /// <summary>
        /// Khoảng cách (km)
        /// Phải >= 0
        /// </summary>
        [Required(ErrorMessage = "Khoảng cách không được để trống")]
        [Range(0, double.MaxValue, ErrorMessage = "Khoảng cách không được âm")]
        public double Distance { get; set; }
        
        // ========== THÔNG SỐ ĐẶC BIỆT ==========
        
        /// <summary>
        /// Hàng dễ vỡ
        /// </summary>
        public bool IsFragile { get; set; } = false;

        /// <summary>
        /// Hàng trị giá cao
        /// </summary>
        public bool IsValuable { get; set; } = false;

        /// <summary>
        /// Hàng cồng kềnh (xe, v.v.)
        /// </summary>
        public bool IsVehicle { get; set; } = false;

        /// <summary>
        /// Thu tiền hộ (COD)
        /// </summary>
        public bool CollectMoney { get; set; } = false;

        /// <summary>
        /// Số tiền COD (nếu chọn)
        /// Phải > 0 và <= 50,000,000 VND
        /// </summary>
        [Range(0, 50000000, ErrorMessage = "Tiền COD phải từ 0 đến 50,000,000 VND")]
        public decimal CollectionAmount { get; set; }
        
        // ========== THANH TOÁN ==========
        
        /// <summary>
        /// Phương thức thanh toán
        /// Options: COD, Online, Momo
        /// </summary>
        [Required(ErrorMessage = "Phương thức thanh toán phải được chọn")]
        public PaymentMethod PaymentMethod { get; set; }

        /// <summary>
        /// Loại giao hàng
        /// Options: Standard, Express
        /// </summary>
        [Required(ErrorMessage = "Loại giao hàng phải được chọn")]
        public DeliveryType DeliveryType { get; set; }
        
        /// <summary>
        /// Cổng thanh toán (Momo, VNPay, etc.) - Chỉ dùng khi PaymentMethod = Online
        /// </summary>
        public string? PaymentGateway { get; set; }
        
        /// <summary>
        /// Ghi chú
        /// Optional, max 500 ký tự
        /// </summary>
        [MaxLength(500, ErrorMessage = "Ghi chú không được vượt quá 500 ký tự")]
        public string Notes { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO trả về sau khi tạo đơn hàng thành công
    /// </summary>
    public class CreateOrderResponseDto
    {
        public int OrderId { get; set; }
        public string OrderCode { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string DeliveryAddress { get; set; } = string.Empty;
        public decimal ShippingFee { get; set; }
        public decimal? CODAmount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string DeliveryType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public string Message { get; set; } = "Tạo đơn hàng thành công";
    }
}
