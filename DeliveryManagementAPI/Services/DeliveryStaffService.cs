using DeliveryManagementAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace DeliveryManagementAPI.Services
{
    /// <summary>
    /// Service để quản lý nhân viên giao hàng sử dụng Entity Framework
    /// </summary>
    public class DeliveryStaffService
    {
        private readonly DeliveryDbContext _context;

        public DeliveryStaffService(DeliveryDbContext context)
        {
            _context = context;
        }

        // Lấy tất cả nhân viên
        public async Task<List<DeliveryStaff>> GetAllStaffAsync()
        {
            return await _context.DeliveryStaffs.ToListAsync();
        }

        // Lấy nhân viên theo ID
        public async Task<DeliveryStaff?> GetStaffByIdAsync(int staffId)
        {
            return await _context.DeliveryStaffs.FindAsync(staffId);
        }

        // Thêm nhân viên mới
        public async Task<DeliveryStaff> AddStaffAsync(DeliveryStaff staff)
        {
            _context.DeliveryStaffs.Add(staff);
            await _context.SaveChangesAsync();
            return staff;
        }

        // Cập nhật nhân viên
        public async Task<DeliveryStaff?> UpdateStaffAsync(DeliveryStaff staff)
        {
            var existingStaff = await _context.DeliveryStaffs.FindAsync(staff.StaffId);
            if (existingStaff == null)
            {
                return null;
            }

            _context.Entry(existingStaff).CurrentValues.SetValues(staff);
            await _context.SaveChangesAsync();
            return existingStaff;
        }

        // Xóa nhân viên
        public async Task<bool> DeleteStaffAsync(int staffId)
        {
            var staff = await _context.DeliveryStaffs.FindAsync(staffId);
            if (staff == null)
            {
                return false;
            }

            _context.DeliveryStaffs.Remove(staff);
            await _context.SaveChangesAsync();
            return true;
        }

        // Cập nhật trạng thái sẵn sàng
        public async Task<bool> UpdateAvailabilityAsync(int staffId, bool isAvailable)
        {
            var staff = await _context.DeliveryStaffs.FindAsync(staffId);
            if (staff == null)
            {
                return false;
            }

            staff.IsAvailable = isAvailable;
            await _context.SaveChangesAsync();
            return true;
        }

        // Lấy nhân viên đang rảnh
        public async Task<List<DeliveryStaff>> GetAvailableStaffAsync()
        {
            return await _context.DeliveryStaffs
                .Where(s => s.IsAvailable)
                .ToListAsync();
        }

        // Đếm số nhân viên
        public async Task<int> CountStaffAsync()
        {
            return await _context.DeliveryStaffs.CountAsync();
        }

        // Đếm số nhân viên đang rảnh
        public async Task<int> CountAvailableStaffAsync()
        {
            return await _context.DeliveryStaffs.CountAsync(s => s.IsAvailable);
        }

        // Tìm nhân viên theo họ tên
        public async Task<DeliveryStaff?> GetByFullNameAsync(string fullName)
        {
            return await _context.DeliveryStaffs.FirstOrDefaultAsync(s => s.FullName == fullName);
        }

        // Tìm nhân viên theo số điện thoại
        public async Task<DeliveryStaff?> GetByPhoneAsync(string phoneNumber)
        {
            return await _context.DeliveryStaffs.FirstOrDefaultAsync(s => s.PhoneNumber == phoneNumber);
        }
    }
}
