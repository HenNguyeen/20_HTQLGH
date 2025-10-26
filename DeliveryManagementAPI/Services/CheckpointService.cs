using DeliveryManagementAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace DeliveryManagementAPI.Services
{
    /// <summary>
    /// Service để quản lý điểm check-in sử dụng Entity Framework
    /// </summary>
    public class CheckpointService
    {
        private readonly DeliveryDbContext _context;

        public CheckpointService(DeliveryDbContext context)
        {
            _context = context;
        }

        // Lấy tất cả checkpoint của một đơn hàng
        public async Task<List<LocationCheckpoint>> GetCheckpointsByOrderIdAsync(int orderId)
        {
            return await _context.LocationCheckpoints
                .Where(c => c.OrderId == orderId)
                .OrderByDescending(c => c.CheckInTime)
                .ToListAsync();
        }

        // Thêm checkpoint mới
        public async Task<LocationCheckpoint> AddCheckpointAsync(LocationCheckpoint checkpoint)
        {
            checkpoint.CheckInTime = DateTime.Now;
            _context.LocationCheckpoints.Add(checkpoint);
            await _context.SaveChangesAsync();
            return checkpoint;
        }

        // Lấy checkpoint mới nhất của đơn hàng
        public async Task<LocationCheckpoint?> GetLatestCheckpointAsync(int orderId)
        {
            return await _context.LocationCheckpoints
                .Where(c => c.OrderId == orderId)
                .OrderByDescending(c => c.CheckInTime)
                .FirstOrDefaultAsync();
        }

        // Xóa tất cả checkpoint của đơn hàng
        public async Task<bool> DeleteCheckpointsByOrderIdAsync(int orderId)
        {
            var checkpoints = await _context.LocationCheckpoints
                .Where(c => c.OrderId == orderId)
                .ToListAsync();

            if (checkpoints.Count == 0)
            {
                return false;
            }

            _context.LocationCheckpoints.RemoveRange(checkpoints);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
