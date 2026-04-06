namespace DeliveryManagementAPI.Models.States
{
    /// <summary>
    /// Trạng thái "Đã Nhận - Đang Giao"
    /// Đơn hàng đang được giao, nhân viên đang trên đường
    /// </summary>
    public class InTransitOrderState : IOrderState
    {
        public string StateName => "In Transit (Đang Giao)";
        public OrderStatus Status => OrderStatus.DaNhanDangGiao;

        public bool CanAssign(Order order) => false; // Không thể gán lại khi đang giao

        public void Assign(Order order, DeliveryStaff staff) 
            => throw new InvalidOperationException($"Không thể gán nhân viên khi đơn hàng {StateName}");

        public bool CanStartDelivery(Order order) => false; // Đã bắt đầu rồi

        public void StartDelivery(Order order) 
            => throw new InvalidOperationException($"Đơn hàng đã {StateName}");

        public bool CanComplete(Order order) => true; // Có thể hoàn tất

        public void Complete(Order order)
        {
            if (!CanComplete(order))
                throw new InvalidOperationException($"Không thể hoàn tất giao từ {StateName}");

            order.DeliveredDate = DateTime.Now;
            order.Status = OrderStatus.DaGiao;
            order.ConfirmedReceived = false; // Chờ khách xác nhận
            order.OrderState = new DeliveredOrderState();
        }

        public bool CanCancel(Order order) => false; // Không thể hủy khi đang giao

        public void Cancel(Order order, string reason) 
            => throw new InvalidOperationException($"Không thể hủy đơn hàng khi {StateName}");

        public List<string> GetAllowedActions() => new()
        {
            "Hoàn tất giao hàng",
            "Cập nhật vị trí (Checkpoint)"
        };
    }
}
