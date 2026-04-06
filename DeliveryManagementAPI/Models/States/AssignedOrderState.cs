namespace DeliveryManagementAPI.Models.States
{
    /// <summary>
    /// Trạng thái "Đã Nhận - Chưa Giao"
    /// Đơn hàng đã được nhân viên nhận, nhưng chưa bắt đầu giao hàng
    /// </summary>
    public class AssignedOrderState : IOrderState
    {
        public string StateName => "Assigned (Đã Nhận - Chưa Giao)";
        public OrderStatus Status => OrderStatus.DaNhanChuaGiao;

        public bool CanAssign(Order order) => true; // Có thể gán lại cho nhân viên khác

        public void Assign(Order order, DeliveryStaff staff)
        {
            if (!CanAssign(order))
                throw new InvalidOperationException($"Không thể gán đơn hàng từ trạng thái {StateName}");

            order.AssignedStaff = staff;
            order.AssignedStaffId = staff.StaffId.ToString(); // Lưu StaffId dưới dạng string
            order.Notes += $"\n[GIAO LẠI] Giao lại cho {staff.FullName} - {DateTime.Now:dd/MM/yyyy HH:mm:ss}";
            
            // Vẫn ở trạng thái này
            order.Status = OrderStatus.DaNhanChuaGiao;
        }

        public bool CanStartDelivery(Order order) 
            => order.AssignedStaff != null; // Phải có nhân viên

        public void StartDelivery(Order order)
        {
            if (!CanStartDelivery(order))
                throw new InvalidOperationException($"Không thể bắt đầu giao: chưa gán nhân viên hoặc không ở trạng thái {StateName}");

            order.DeliveryStartDate = DateTime.Now;
            order.Status = OrderStatus.DaNhanDangGiao;
            order.OrderState = new InTransitOrderState();
        }

        public bool CanComplete(Order order) => false; // Chưa bắt đầu giao

        public void Complete(Order order) 
            => throw new InvalidOperationException($"Không thể hoàn tất giao từ {StateName}. Phải bắt đầu giao trước.");

        public bool CanCancel(Order order) => true; // Có thể hủy khi chưa giao

        public void Cancel(Order order, string reason)
        {
            if (!CanCancel(order))
                throw new InvalidOperationException($"Không thể hủy đơn hàng từ {StateName}");

            order.Status = OrderStatus.ChuaNhan; // Chuyển lại sang pending
            order.AssignedStaff = null;
            order.AssignedStaffId = null;
            order.Notes += $"\n[HỦY] {reason} - {DateTime.Now:dd/MM/yyyy HH:mm:ss}";
        }

        public List<string> GetAllowedActions() => new()
        {
            "Bắt đầu giao hàng",
            "Gán lại nhân viên",
            "Hủy đơn hàng"
        };
    }
}
