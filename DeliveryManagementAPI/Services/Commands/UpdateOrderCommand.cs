using DeliveryManagementAPI.Models;

namespace DeliveryManagementAPI.Services.Commands
{
    /// <summary>
    /// Command để cập nhật thông tin đơn hàng
    /// </summary>
    public class UpdateOrderCommand : IOrderCommand
    {
        private readonly Order _order;

        public OrderCommandType CommandType => OrderCommandType.Update;

        public UpdateOrderCommand(Order order)
        {
            _order = order ?? throw new ArgumentNullException(nameof(order));
        }

        public async Task ExecuteAsync(OrderService orderService)
        {
            if (_order == null)
                throw new InvalidOperationException("Order không được null");

            if (_order.OrderId <= 0)
                throw new InvalidOperationException("OrderId phải lớn hơn 0");

            // Validate order exists
            var existingOrder = await orderService.GetOrderByIdAsync(_order.OrderId);
            if (existingOrder == null)
                throw new InvalidOperationException($"Đơn hàng với ID {_order.OrderId} không tồn tại");

            // Thực thi command - cập nhật đơn hàng
            await orderService.UpdateOrderAsync(_order);
        }

        public string GetDescription()
        {
            return $"Cập nhật đơn hàng ID {_order.OrderId}: {_order.OrderCode}";
        }
    }
}
