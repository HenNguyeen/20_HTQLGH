namespace DeliveryManagementAPI.Models
{
    /// <summary>
    /// Thông tin khách hàng
    /// </summary>
    public class Customer
    {
        [System.ComponentModel.DataAnnotations.Key]
        public int CustomerId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Ward { get; set; } = string.Empty;      // Phường/Xã
        public string District { get; set; } = string.Empty;  // Quận/Huyện
        public string City { get; set; } = string.Empty;      // Thành phố
    }
}
