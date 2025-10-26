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
    }
}
