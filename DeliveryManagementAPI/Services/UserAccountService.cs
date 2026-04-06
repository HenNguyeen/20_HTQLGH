using DeliveryManagementAPI.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using BCrypt.Net;

namespace DeliveryManagementAPI.Services
{
    /// <summary>
    /// Service để quản lý tài khoản người dùng sử dụng Entity Framework
    /// </summary>
    public class UserAccountService
    {
        private readonly DeliveryDbContext _context;

        public UserAccountService(DeliveryDbContext context)
        {
            _context = context;
        }

        // Lấy tất cả người dùng
        public async Task<List<UserAccount>> GetAllAsync()
        {
            return await _context.UserAccounts.ToListAsync();
        }

        // Lấy người dùng theo username
        public async Task<UserAccount?> GetByUsernameAsync(string username)
        {
            return await _context.UserAccounts
                .FirstOrDefaultAsync(u => u.Username == username);
        }

        // Lấy người dùng theo email
        public async Task<UserAccount?> GetByEmailAsync(string email)
        {
            return await _context.UserAccounts
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        // Lấy người dùng theo ID
        public async Task<UserAccount?> GetByIdAsync(int id)
        {
            return await _context.UserAccounts.FindAsync(id);
        }

        // Kiểm tra mật khẩu
        public bool CheckPassword(UserAccount user, string password)
        {
            try
            {
                return BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
            }
            catch
            {
                // In case of any error (e.g., invalid hash), return false
                return false;
            }
        }

        // Xác thực người dùng
        public async Task<UserAccount?> AuthenticateAsync(string username, string password)
        {
            var user = await GetByUsernameAsync(username);
            if (user == null) return null;
            return CheckPassword(user, password) ? user : null;
        }

        // Kiểm tra username đã tồn tại
        public async Task<bool> UsernameExistsAsync(string username)
        {
            return await _context.UserAccounts.AnyAsync(u => u.Username == username);
        }

        // Kiểm tra email đã tồn tại
        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.UserAccounts.AnyAsync(u => u.Email == email);
        }

        // Đăng ký người dùng mới
        public async Task<UserAccount> RegisterAsync(UserAccount user, string password)
        {
            user.PasswordHash = HashPassword(password);
            _context.UserAccounts.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        // Tạo user mới (dùng cho admin tạo tài khoản shipper)
        public async Task<UserAccount> CreateUserAsync(UserAccount user)
        {
            _context.UserAccounts.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        // Cập nhật thông tin người dùng
        public async Task UpdateAsync(UserAccount user)
        {
            _context.UserAccounts.Update(user);
            await _context.SaveChangesAsync();
        }

        // Xóa người dùng
        public async Task<bool> DeleteAsync(int id)
        {
            var user = await GetByIdAsync(id);
            if (user == null) return false;
            _context.UserAccounts.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }

        // Hash mật khẩu sử dụng bcrypt (an toàn hơn SHA256)
        public static string HashPassword(string password)
        {
            // Using bcrypt with default cost (10 rounds)
            // This is more secure than SHA256 and provides built-in salt generation
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        // Thiết lập token reset mật khẩu
        public async Task SetResetTokenAsync(string email, string token, DateTime expiry)
        {
            var user = await GetByEmailAsync(email);
            if (user != null)
            {
                user.PasswordResetToken = token;
                user.ResetTokenExpiry = expiry;
                await _context.SaveChangesAsync();
            }
        }

        // Lấy người dùng theo reset token
        public async Task<UserAccount?> GetByResetTokenAsync(string token)
        {
            return await _context.UserAccounts
                .FirstOrDefaultAsync(u => u.PasswordResetToken == token && u.ResetTokenExpiry > DateTime.UtcNow);
        }

        // Reset mật khẩu
        public async Task ResetPasswordAsync(UserAccount user, string newPassword)
        {
            user.PasswordHash = HashPassword(newPassword);
            user.PasswordResetToken = null;
            user.ResetTokenExpiry = null;
            await _context.SaveChangesAsync();
        }

        // Two-Factor Authentication Methods

        // Enable 2FA for a user
        public async Task<bool> EnableTwoFactorAsync(int userId)
        {
            var user = await GetByIdAsync(userId);
            if (user == null) return false;

            user.TwoFactorEnabled = true;
            await _context.SaveChangesAsync();
            return true;
        }

        // Disable 2FA for a user
        public async Task<bool> DisableTwoFactorAsync(int userId)
        {
            var user = await GetByIdAsync(userId);
            if (user == null) return false;

            user.TwoFactorEnabled = false;
            user.TwoFactorCode = null;
            user.TwoFactorCodeExpiry = null;
            await _context.SaveChangesAsync();
            return true;
        }

        // Set 2FA code for a user
        public async Task SetTwoFactorCodeAsync(int userId, string code, DateTime expiry)
        {
            var user = await GetByIdAsync(userId);
            if (user != null)
            {
                user.TwoFactorCode = code;
                user.TwoFactorCodeExpiry = expiry;
                await _context.SaveChangesAsync();
            }
        }

        // Verify 2FA code
        public async Task<bool> VerifyTwoFactorCodeAsync(int userId, string code)
        {
            var user = await GetByIdAsync(userId);
            if (user == null) return false;

            if (user.TwoFactorCode == code && user.TwoFactorCodeExpiry > DateTime.UtcNow)
            {
                // Clear the code after successful verification
                user.TwoFactorCode = null;
                user.TwoFactorCodeExpiry = null;
                await _context.SaveChangesAsync();
                return true;
            }

            return false;
        }
    }
}