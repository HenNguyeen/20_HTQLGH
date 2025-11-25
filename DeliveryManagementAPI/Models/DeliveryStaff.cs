namespace DeliveryManagementAPI.Models
{
    /// <summary>
    /// Thông tin nhân viên giao hàng
    /// </summary>
    public class DeliveryStaff
    {
        
    [System.ComponentModel.DataAnnotations.Key]
    public int StaffId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string VehicleType { get; set; } = string.Empty; // Loại phương tiện
        public string VehiclePlate { get; set; } = string.Empty; // Biển số xe
        public bool IsAvailable { get; set; } = true;
        
        // GPS Tracking - Vị trí realtime
        public double? CurrentLatitude { get; set; }  // Vĩ độ hiện tại
        public double? CurrentLongitude { get; set; } // Kinh độ hiện tại
        public DateTime? LastLocationUpdate { get; set; } // Lần cập nhật cuối
    }
}
