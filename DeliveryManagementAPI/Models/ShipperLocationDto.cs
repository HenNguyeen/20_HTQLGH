namespace DeliveryManagementAPI.Models
{
    /// <summary>
    /// DTO để shipper cập nhật vị trí realtime
    /// </summary>
    public class ShipperLocationDto
    {
        public int OrderId { get; set; }
        public int StaffId { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
