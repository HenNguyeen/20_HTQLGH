namespace DeliveryManagementAPI.Models.States
{
    /// <summary>
    /// Trạng thái "Chưa Nhận / Chờ Xử Lý"
    /// Đơn hàng vừa được tạo, chưa được nhân viên nhận
    /// </summary>
    public class PendingOrderState : IOrderState
    {
        public string StateName => "Pending (Chưa Nhận)";
        public OrderStatus Status => OrderStatus.ChuaNhan;

        public bool CanAssign(Order order) => true; // Luôn có thể gán từ trạng thái này

        public void Assign(Order order, DeliveryStaff staff)
        {
            if (!CanAssign(order))
                throw new InvalidOperationException($"Không thể gán đơn hàng từ trạng thái {StateName}");

            order.AssignedStaff = staff;
            order.AssignedStaffId = staff.StaffId.ToString(); // Lưu StaffId dưới dạng string
            order.ReceivedDate = DateTime.Now;
            
            // Chuyển sang trạng thái "Đã Nhận - Chưa Giao"
            order.Status = OrderStatus.DaNhanChuaGiao;
            order.OrderState = new AssignedOrderState();
        }

        public bool CanStartDelivery(Order order) => false; // Phải gán trước

        public void StartDelivery(Order order) 
            => throw new InvalidOperationException($"Không thể bắt đầu giao từ {StateName}. Phải gán nhân viên trước.");

        public bool CanComplete(Order order) => false; // Chưa bắt đầu

        public void Complete(Order order) 
            => throw new InvalidOperationException($"Không thể hoàn tất giao từ {StateName}");

        public bool CanCancel(Order order) => true; // Có thể hủy khi chưa nhận

        public void Cancel(Order order, string reason)
        {
            if (!CanCancel(order))
                throw new InvalidOperationException($"Không thể hủy đơn hàng từ {StateName}");

            order.Status = OrderStatus.ChuaNhan; // Vẫn là chưa nhận - nhưng đánh dấu là hủy
            order.Notes += $"\n[HỦY] {reason} - {DateTime.Now:dd/MM/yyyy HH:mm:ss}";
        }

        public List<string> GetAllowedActions() => new()
        {
            "Gán nhân viên",
            "Hủy đơn hàng"
        };
    }
}
