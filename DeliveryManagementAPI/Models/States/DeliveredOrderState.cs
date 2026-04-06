namespace DeliveryManagementAPI.Models.States
{
    /// <summary>
    /// Trạng thái "Đã Giao"
    /// Đơn hàng đã được giao cho khách, chờ xác nhận từ khách
    /// </summary>
    public class DeliveredOrderState : IOrderState
    {
        public string StateName => "Delivered (Đã Giao)";
        public OrderStatus Status => OrderStatus.DaGiao;

        public bool CanAssign(Order order) => false; // Không thể gán lại

        public void Assign(Order order, DeliveryStaff staff) 
            => throw new InvalidOperationException($"Không thể gán nhân viên khi đơn hàng {StateName}");

        public bool CanStartDelivery(Order order) => false; // Đã giao rồi

        public void StartDelivery(Order order) 
            => throw new InvalidOperationException($"Đơn hàng đã {StateName}");

        public bool CanComplete(Order order) => false; // Đã hoàn tất

        public void Complete(Order order) 
            => throw new InvalidOperationException($"Đơn hàng đã {StateName}");

        public bool CanCancel(Order order) => false; // Không thể hủy khi đã giao

        public void Cancel(Order order, string reason) 
            => throw new InvalidOperationException($"Không thể hủy đơn hàng khi {StateName}");

        /// <summary>
        /// Xác nhận rằng khách đã nhận hàng
        /// </summary>
        public void ConfirmReceived(Order order)
        {
            order.ConfirmedReceived = true;
            order.ConfirmedAt = DateTime.Now;
            order.Notes += $"\n[XÁC NHẬN] Khách đã xác nhận nhận hàng - {DateTime.Now:dd/MM/yyyy HH:mm:ss}";
        }

        public List<string> GetAllowedActions() => new()
        {
            "Xác nhận nhận hàng (khách)",
            "Xem chi tiết đơn hàng"
        };
    }
}
