namespace DeliveryManagementAPI.Models
{
    /// <summary>
    /// Trạng thái đơn hàng
    /// </summary>
    public enum OrderStatus
    {
        ChuaNhan = 0,           // Chưa nhận
        DaNhanChuaGiao = 1,     // Đã nhận - Chưa giao
        DaNhanDangGiao = 2,     // Đã nhận - Đang giao
        DaGiao = 3              // Đã giao
    }
}
