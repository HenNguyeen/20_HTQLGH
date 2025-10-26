using DeliveryManagementAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace DeliveryManagementAPI.Services
{
    /// <summary>
    /// Service để quản lý đơn hàng sử dụng Entity Framework
    /// </summary>
    public class OrderService
    {
        private readonly DeliveryDbContext _context;

        public OrderService(DeliveryDbContext context)
        {
            _context = context;
        }

        // Lấy tất cả đơn hàng
        public async Task<List<Order>> GetAllOrdersAsync()
        {
            return await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.AssignedStaff)
                .OrderByDescending(o => o.CreatedDate)
                .ToListAsync();
        }

        // Lấy đơn hàng theo ID
        public async Task<Order?> GetOrderByIdAsync(int orderId)
        {
            return await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.AssignedStaff)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);
        }

        // Lấy đơn hàng theo OrderCode
        public async Task<Order?> GetOrderByCodeAsync(string orderCode)
        {
            return await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.AssignedStaff)
                .FirstOrDefaultAsync(o => o.OrderCode == orderCode);
        }

        // Thêm đơn hàng mới
        public async Task<Order> AddOrderAsync(Order order)
        {
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            return order;
        }

        // Cập nhật đơn hàng
        public async Task<Order?> UpdateOrderAsync(Order order)
        {
            var existingOrder = await _context.Orders.FindAsync(order.OrderId);
            if (existingOrder == null)
            {
                return null;
            }

            _context.Entry(existingOrder).CurrentValues.SetValues(order);
            await _context.SaveChangesAsync();
            return existingOrder;
        }

        // Xóa đơn hàng
        public async Task<bool> DeleteOrderAsync(int orderId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
            {
                return false;
            }

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();
            return true;
        }

        // Cập nhật trạng thái đơn hàng
        public async Task<bool> UpdateOrderStatusAsync(int orderId, OrderStatus status)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
            {
                return false;
            }

            order.Status = status;

            // Cập nhật các ngày tháng tương ứng
            switch (status)
            {
                case OrderStatus.DaNhanChuaGiao:
                    order.ReceivedDate = DateTime.Now;
                    break;
                case OrderStatus.DaNhanDangGiao:
                    order.DeliveryStartDate = DateTime.Now;
                    break;
                case OrderStatus.DaGiao:
                    order.DeliveredDate = DateTime.Now;
                    break;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        // Gán nhân viên cho đơn hàng
        public async Task<bool> AssignStaffAsync(int orderId, int staffId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            var staff = await _context.DeliveryStaffs.FindAsync(staffId);

            if (order == null || staff == null)
            {
                return false;
            }

            order.AssignedStaffId = staffId.ToString();
            order.AssignedStaff = staff;
            await _context.SaveChangesAsync();
            return true;
        }

        // Lấy đơn hàng theo nhân viên
        public async Task<List<Order>> GetOrdersByStaffIdAsync(int staffId)
        {
            return await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.AssignedStaff)
                .Where(o => o.AssignedStaffId == staffId.ToString())
                .OrderByDescending(o => o.CreatedDate)
                .ToListAsync();
        }

        // Lấy đơn hàng theo trạng thái
        public async Task<List<Order>> GetOrdersByStatusAsync(OrderStatus status)
        {
            return await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.AssignedStaff)
                .Where(o => o.Status == status)
                .OrderByDescending(o => o.CreatedDate)
                .ToListAsync();
        }

        // Lấy đơn hàng do một user tạo
        public async Task<List<Order>> GetOrdersByCreatorAsync(int userId)
        {
            return await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.AssignedStaff)
                .Where(o => o.CreatedByUserId == userId)
                .OrderByDescending(o => o.CreatedDate)
                .ToListAsync();
        }

        // Đếm số đơn hàng theo trạng thái
        public async Task<int> CountOrdersByStatusAsync(OrderStatus status)
        {
            return await _context.Orders.CountAsync(o => o.Status == status);
        }

        // Tính tổng doanh thu
        public async Task<decimal> GetTotalRevenueAsync()
        {
            return await _context.Orders
                .Where(o => o.Status == OrderStatus.DaGiao && o.IsPaid)
                .SumAsync(o => o.ShippingFee);
        }
    }
}
