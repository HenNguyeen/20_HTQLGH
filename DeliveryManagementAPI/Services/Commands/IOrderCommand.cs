namespace DeliveryManagementAPI.Services.Commands
{
    /// <summary>
    /// Command Pattern Interface
    /// Mỗi command đại diện cho một hành động trên Order
    /// </summary>
    public interface IOrderCommand
    {
        /// <summary>
        /// Thực thi command
        /// </summary>
        Task ExecuteAsync(OrderService orderService);

        /// <summary>
        /// Lấy mô tả của command
        /// </summary>
        string GetDescription();

        /// <summary>
        /// Command Type (để logging/audit)
        /// </summary>
        OrderCommandType CommandType { get; }
    }

    /// <summary>
    /// Các loại command Order
    /// </summary>
    public enum OrderCommandType
    {
        Create,
        Update,
        Delete,
        UpdateStatus,
        AssignStaff,
        UnAssignStaff,
        ConfirmReceived
    }
}
