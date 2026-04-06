namespace DeliveryManagementAPI.Services.Commands
{
    /// <summary>
    /// Command để gán nhân viên cho đơn hàng
    /// </summary>
    public class AssignStaffCommand : IOrderCommand
    {
        private readonly int _orderId;
        private readonly int _staffId;

        public OrderCommandType CommandType => OrderCommandType.AssignStaff;

        public AssignStaffCommand(int orderId, int staffId)
        {
            if (orderId <= 0)
                throw new ArgumentException("OrderId phải lớn hơn 0", nameof(orderId));

            if (staffId <= 0)
                throw new ArgumentException("StaffId phải lớn hơn 0", nameof(staffId));

            _orderId = orderId;
            _staffId = staffId;
        }

        public async Task ExecuteAsync(OrderService orderService)
        {
            // Validate order exists
            var existingOrder = await orderService.GetOrderByIdAsync(_orderId);
            if (existingOrder == null)
                throw new InvalidOperationException($"Đơn hàng với ID {_orderId} không tồn tại");

            // Thực thi command - gán nhân viên
            var result = await orderService.AssignStaffAsync(_orderId, _staffId);
            if (!result)
                throw new InvalidOperationException($"Không thể gán nhân viên ID {_staffId} cho đơn hàng ID {_orderId}");
        }

        public string GetDescription()
        {
            return $"Gán nhân viên ID {_staffId} cho đơn hàng ID {_orderId}";
        }
    }
}
