using DeliveryManagementAPI.Models.States;
using Microsoft.EntityFrameworkCore;

namespace DeliveryManagementAPI.Services
{
    /// <summary>
    /// Service quản lý chuyển đổi trạng thái đơn hàng (State Pattern)
    /// Cung cấp các phương thức an toàn để chuyển đổi trạng thái
    /// </summary>
    public class OrderStateService
    {
        private readonly DeliveryDbContext _context;
        private readonly ILogger<OrderStateService> _logger;

        public OrderStateService(DeliveryDbContext context, ILogger<OrderStateService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Gán đơn hàng cho nhân viên giao hàng
        /// </summary>
        public async Task<bool> AssignOrderToStaffAsync(int orderId, string staffId)
        {
            try
            {
                var order = await _context.Orders.Include(o => o.OrderState).FirstOrDefaultAsync(o => o.OrderId == orderId);
                if (order == null)
                {
                    _logger.LogWarning($"Không tìm thấy đơn hàng ID: {orderId}");
                    return false;
                }

                // Khởi tạo state nếu chưa có
                order.InitializeState();

                if (order.OrderState == null || !order.OrderState.CanAssign(order))
                {
                    _logger.LogWarning($"Không thể gán đơn hàng {order.OrderCode} từ trạng thái {order.Status}");
                    return false;
                }

                var staff = await _context.DeliveryStaffs.FindAsync(staffId);
                if (staff == null)
                {
                    _logger.LogWarning($"Không tìm thấy nhân viên ID: {staffId}");
                    return false;
                }

                order.OrderState.Assign(order, staff);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"✅ Gán đơn hàng {order.OrderCode} cho nhân viên {staff.FullName}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Lỗi gán đơn hàng: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Bắt đầu giao hàng
        /// </summary>
        public async Task<bool> StartDeliveryAsync(int orderId)
        {
            try
            {
                var order = await _context.Orders.FirstOrDefaultAsync(o => o.OrderId == orderId);
                if (order == null)
                {
                    _logger.LogWarning($"Không tìm thấy đơn hàng ID: {orderId}");
                    return false;
                }

                order.InitializeState();

                if (order.OrderState == null || !order.OrderState.CanStartDelivery(order))
                {
                    _logger.LogWarning($"Không thể bắt đầu giao đơn hàng {order.OrderCode}");
                    return false;
                }

                order.OrderState.StartDelivery(order);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"✅ Bắt đầu giao hàng {order.OrderCode}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Lỗi bắt đầu giao hàng: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Hoàn tát giao hàng - chuyển sang "Đã Giao"
        /// </summary>
        public async Task<bool> CompleteDeliveryAsync(int orderId)
        {
            try
            {
                var order = await _context.Orders.FirstOrDefaultAsync(o => o.OrderId == orderId);
                if (order == null)
                {
                    _logger.LogWarning($"Không tìm thấy đơn hàng ID: {orderId}");
                    return false;
                }

                order.InitializeState();

                if (order.OrderState == null || !order.OrderState.CanComplete(order))
                {
                    _logger.LogWarning($"Không thể hoàn tát giao đơn hàng {order.OrderCode}");
                    return false;
                }

                order.OrderState.Complete(order);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"✅ Hoàn tát giao hàng {order.OrderCode}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Lỗi hoàn tát giao hàng: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Hủy đơn hàng
        /// </summary>
        public async Task<bool> CancelOrderAsync(int orderId, string reason)
        {
            try
            {
                var order = await _context.Orders.FirstOrDefaultAsync(o => o.OrderId == orderId);
                if (order == null)
                {
                    _logger.LogWarning($"Không tìm thấy đơn hàng ID: {orderId}");
                    return false;
                }

                order.InitializeState();

                if (order.OrderState == null || !order.OrderState.CanCancel(order))
                {
                    _logger.LogWarning($"Không thể hủy đơn hàng {order.OrderCode} từ trạng thái {order.Status}");
                    return false;
                }

                order.OrderState.Cancel(order, reason);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"✅ Hủy đơn hàng {order.OrderCode}. Lý do: {reason}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Lỗi hủy đơn hàng: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Lấy danh sách các hành động được phép cho đơn hàng
        /// </summary>
        public async Task<List<string>> GetAllowedActionsAsync(int orderId)
        {
            try
            {
                var order = await _context.Orders.FirstOrDefaultAsync(o => o.OrderId == orderId);
                if (order == null)
                    return new List<string>();

                order.InitializeState();
                return order.OrderState?.GetAllowedActions() ?? new List<string>();
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Lỗi lấy hành động cho phép: {ex.Message}");
                return new List<string>();
            }
        }

        /// <summary>
        /// Lấy trạng thái hiện tại của đơn hàng (IOrderState chi tiết)
        /// </summary>
        public async Task<IOrderState?> GetCurrentStateAsync(int orderId)
        {
            try
            {
                var order = await _context.Orders.FirstOrDefaultAsync(o => o.OrderId == orderId);
                if (order == null)
                    return null;

                order.InitializeState();
                return order.OrderState;
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Lỗi lấy trạng thái hiện tại: {ex.Message}");
                return null;
            }
        }
    }
}
