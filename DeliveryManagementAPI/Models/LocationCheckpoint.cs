namespace DeliveryManagementAPI.Models
{
    /// <summary>
    /// Điểm check in vị trí trong quá trình giao hàng
    /// </summary>
    public class LocationCheckpoint
    {
        [System.ComponentModel.DataAnnotations.Key]
        public int CheckpointId { get; set; }
        public int OrderId { get; set; }
        public double Latitude { get; set; }  // Vĩ độ
        public double Longitude { get; set; } // Kinh độ
        public string LocationName { get; set; } = string.Empty;
        public DateTime CheckInTime { get; set; }
        public string Notes { get; set; } = string.Empty;
    }
}
