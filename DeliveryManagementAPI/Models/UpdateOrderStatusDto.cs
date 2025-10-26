namespace DeliveryManagementAPI.Models
{
    /// <summary>
    /// DTO để cập nhật trạng thái đơn hàng
    /// </summary>
    public class UpdateOrderStatusDto
    {
        public string OrderId { get; set; } = string.Empty;
        public OrderStatus Status { get; set; }
        public string? StaffId { get; set; }
        public string Notes { get; set; } = string.Empty;
    }
}
