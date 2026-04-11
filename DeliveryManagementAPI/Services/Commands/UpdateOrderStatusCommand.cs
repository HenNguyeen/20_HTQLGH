using DeliveryManagementAPI.Models;

namespace DeliveryManagementAPI.Services.Commands
{
    /// <summary>
    /// Command để cập nhật trạng thái của đơn hàng
    /// </summary>
    public class UpdateOrderStatusCommand : IOrderCommand
    {
        private readonly int _orderId;
        private readonly OrderStatus _newStatus;

        public OrderCommandType CommandType => OrderCommandType.UpdateStatus;

        public UpdateOrderStatusCommand(int orderId, OrderStatus newStatus)
        {
            if (orderId <= 0)
                throw new ArgumentException("OrderId phải lớn hơn 0", nameof(orderId));

            _orderId = orderId;
            _newStatus = newStatus;
        }

        public async Task ExecuteAsync(OrderService orderService)
        {
            // Validate order exists
            var existingOrder = await orderService.GetOrderByIdAsync(_orderId);
            if (existingOrder == null)
                throw new InvalidOperationException($"Đơn hàng với ID {_orderId} không tồn tại");

            // Validate status transition (Optional - có thể thêm logic validate trạng thái hợp lệ)
            if (!IsValidStatusTransition(existingOrder.Status, _newStatus))
                throw new InvalidOperationException(
                    $"Không thể chuyển từ trạng thái {existingOrder.Status} sang {_newStatus}");

            // Thực thi command - cập nhật trạng thái
            var result = await orderService.UpdateOrderStatusAsync(_orderId, _newStatus);
            if (!result)
                throw new InvalidOperationException($"Không thể cập nhật trạng thái cho đơn hàng ID {_orderId}");
        }

        public string GetDescription()
        {
            return $"Cập nhật trạng thái đơn hàng ID {_orderId} sang {_newStatus}";
        }

        /// <summary>
        /// Kiểm tra xem chuyển đổi trạng thái có hợp lệ không
        /// Quy tắc: ChuaNhan -> DaNhanChuaGiao -> DaNhanDangGiao -> DaGiao (không thể quay lại)
        /// Ngoại lệ: Shipper có thể chuyển từ ChuaNhan sang DaNhanDangGiao trực tiếp (tự gán)
        /// </summary>
        private bool IsValidStatusTransition(OrderStatus currentStatus, OrderStatus newStatus)
        {
            // Nếu cùng trạng thái thì không cần cập nhật
            if (currentStatus == newStatus)
                return false;

            // Định nghĩa các chuyển đổi trạng thái hợp lệ (một chiều)
            var validTransitions = new Dictionary<OrderStatus, List<OrderStatus>>
            {
                { OrderStatus.ChuaNhan, new() { OrderStatus.DaNhanChuaGiao, OrderStatus.DaNhanDangGiao } }, // Allow shipper to directly start delivery
                { OrderStatus.DaNhanChuaGiao, new() { OrderStatus.DaNhanDangGiao } },
                { OrderStatus.DaNhanDangGiao, new() { OrderStatus.DaGiao } },
                { OrderStatus.DaGiao, new() } // Final state, không thể chuyển tiếp
            };

            if (!validTransitions.ContainsKey(currentStatus))
                return false;

            return validTransitions[currentStatus].Contains(newStatus);
        }
    }
}
