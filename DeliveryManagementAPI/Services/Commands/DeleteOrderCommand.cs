namespace DeliveryManagementAPI.Services.Commands
{
    /// <summary>
    /// Command để xóa một đơn hàng
    /// </summary>
    public class DeleteOrderCommand : IOrderCommand
    {
        private readonly int _orderId;

        public OrderCommandType CommandType => OrderCommandType.Delete;

        public DeleteOrderCommand(int orderId)
        {
            if (orderId <= 0)
                throw new ArgumentException("OrderId phải lớn hơn 0", nameof(orderId));

            _orderId = orderId;
        }

        public async Task ExecuteAsync(OrderService orderService)
        {
            // Validate order exists trước khi xóa
            var existingOrder = await orderService.GetOrderByIdAsync(_orderId);
            if (existingOrder == null)
                throw new InvalidOperationException($"Đơn hàng với ID {_orderId} không tồn tại");

            // Thực thi command - xóa đơn hàng
            var result = await orderService.DeleteOrderAsync(_orderId);
            if (!result)
                throw new InvalidOperationException($"Không thể xóa đơn hàng ID {_orderId}");
        }

        public string GetDescription()
        {
            return $"Xóa đơn hàng ID {_orderId}";
        }
    }
}
