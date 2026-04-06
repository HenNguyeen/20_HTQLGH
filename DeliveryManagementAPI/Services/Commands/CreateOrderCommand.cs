using DeliveryManagementAPI.Models;

namespace DeliveryManagementAPI.Services.Commands
{
    /// <summary>
    /// Command để tạo một đơn hàng mới
    /// </summary>
    public class CreateOrderCommand : IOrderCommand
    {
        private readonly Order _order;

        public OrderCommandType CommandType => OrderCommandType.Create;

        public CreateOrderCommand(Order order)
        {
            _order = order ?? throw new ArgumentNullException(nameof(order));
        }

        public async Task ExecuteAsync(OrderService orderService)
        {
            if (_order == null)
                throw new InvalidOperationException("Order không được null");

            // Validate order
            if (_order.CustomerId <= 0)
                throw new InvalidOperationException("CustomerId phải lớn hơn 0");

            if (string.IsNullOrEmpty(_order.OrderCode))
                throw new InvalidOperationException("OrderCode không được rỗng");

            // Set created date
            _order.CreatedDate = DateTime.Now;

            // Thực thi command - tạo đơn hàng
            await orderService.AddOrderAsync(_order);
        }

        public string GetDescription()
        {
            return $"Tạo đơn hàng mới: {_order.OrderCode} cho khách hàng ID {_order.CustomerId}";
        }
    }
}
