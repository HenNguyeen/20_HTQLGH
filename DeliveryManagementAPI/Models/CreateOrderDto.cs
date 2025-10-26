namespace DeliveryManagementAPI.Models
{
    /// <summary>
    /// DTO để tạo đơn hàng mới
    /// </summary>
    public class CreateOrderDto
    {
        public string OrderCode { get; set; } = string.Empty;
        
        // Thông tin khách hàng
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public string DeliveryAddress { get; set; } = string.Empty;
        public string Ward { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        
        // Thông tin hàng hóa
        public string ProductCode { get; set; } = string.Empty;
        public PackageType PackageType { get; set; }
        public double Weight { get; set; }
        public string Size { get; set; } = string.Empty;
        public double Distance { get; set; }
        
        // Thông số đặc biệt
        public bool IsFragile { get; set; }
        public bool IsValuable { get; set; }
        public bool IsVehicle { get; set; }
        public bool CollectMoney { get; set; }
        public decimal CollectionAmount { get; set; }
        
        // Thanh toán
        public PaymentMethod PaymentMethod { get; set; }
        public DeliveryType DeliveryType { get; set; }
        
        public string Notes { get; set; } = string.Empty;
    }
}
