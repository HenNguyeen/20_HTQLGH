namespace DeliveryManagementAPI.Models.States
{
    /// <summary>
    /// Interface cho State Pattern - Quản lý vòng đời đơn hàng
    /// Mỗi state đại diện cho một trạng thái cụ thể của đơn hàng
    /// </summary>
    public interface IOrderState
    {
        /// <summary>
        /// Tên trạng thái (để logging/audit)
        /// </summary>
        string StateName { get; }

        /// <summary>
        /// Giá trị OrderStatus tương ứng
        /// </summary>
        OrderStatus Status { get; }

        /// <summary>
        /// Kiểm tra xem đơn hàng có thể được gán cho nhân viên không
        /// </summary>
        bool CanAssign(Order order);

        /// <summary>
        /// Thực hiện gán đơn hàng cho nhân viên
        /// </summary>
        void Assign(Order order, DeliveryStaff staff);

        /// <summary>
        /// Kiểm tra xem đơn hàng có thể bắt đầu giao không
        /// </summary>
        bool CanStartDelivery(Order order);

        /// <summary>
        /// Thực hiện bắt đầu giao - chuyển sang trạng thái "Đang giao"
        /// </summary>
        void StartDelivery(Order order);

        /// <summary>
        /// Kiểm tra xem đơn hàng có thể hoàn tất giao không
        /// </summary>
        bool CanComplete(Order order);

        /// <summary>
        /// Thực hiện hoàn tất giao - chuyển sang trạng thái "Đã giao"
        /// </summary>
        void Complete(Order order);

        /// <summary>
        /// Kiểm tra xem đơn hàng có thể hủy không
        /// </summary>
        bool CanCancel(Order order);

        /// <summary>
        /// Thực hiện hủy đơn hàng
        /// </summary>
        void Cancel(Order order, string reason);

        /// <summary>
        /// Lấy danh sách các hành động được phép từ state này
        /// </summary>
        List<string> GetAllowedActions();
    }
}
