using DeliveryManagementAPI.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace DeliveryManagementAPI.Services
{
    /// <summary>
    /// Service để quản lý Audit Log (Command Pattern #12)
    /// Ghi lại tất cả các hành động trên Order cho compliance, troubleshooting, tracking
    /// </summary>
    public class AuditLogService
    {
        private readonly DeliveryDbContext _context;
        private readonly ILogger<AuditLogService> _logger;

        public AuditLogService(DeliveryDbContext context, ILogger<AuditLogService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Ghi audit log khi command được thực thi
        /// </summary>
        public async Task LogCommandAsync(
            string commandType,
            string commandDescription,
            int? orderId,
            string? orderCode,
            object? oldValue,
            object? newValue,
            int? userId,
            string? username,
            string? userRole,
            string? ipAddress,
            bool success,
            string? errorMessage,
            long executionTimeMs)
        {
            try
            {
                var auditLog = new AuditLog
                {
                    CommandType = commandType,
                    CommandDescription = commandDescription,
                    OrderId = orderId,
                    OrderCode = orderCode,
                    OldValue = oldValue != null ? JsonConvert.SerializeObject(oldValue, Formatting.Indented) : null,
                    NewValue = newValue != null ? JsonConvert.SerializeObject(newValue, Formatting.Indented) : null,
                    UserId = userId,
                    Username = username,
                    UserRole = userRole,
                    IPAddress = ipAddress,
                    CreatedDate = DateTime.Now,
                    Success = success,
                    ErrorMessage = errorMessage,
                    ExecutionTimeMs = executionTimeMs
                };

                _context.AuditLogs.Add(auditLog);
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    $"[AuditLog] Recorded: {commandType} | Order: {orderCode} | User: {username} | Success: {success} | Time: {executionTimeMs}ms");
            }
            catch (Exception ex)
            {
                _logger.LogError($"[AuditLog] Failed to save audit log: {ex.Message}");
                // Không throw exception để tránh ảnh hưởng đến main flow
            }
        }

        /// <summary>
        /// Lấy audit logs cho một đơn hàng
        /// </summary>
        public async Task<List<AuditLog>> GetOrderAuditLogsAsync(int orderId)
        {
            return await _context.AuditLogs
                .Where(a => a.OrderId == orderId)
                .OrderByDescending(a => a.CreatedDate)
                .ToListAsync();
        }

        /// <summary>
        /// Lấy audit logs theo ngày
        /// </summary>
        public async Task<List<AuditLog>> GetAuditLogsByDateAsync(DateTime from, DateTime to)
        {
            return await _context.AuditLogs
                .Where(a => a.CreatedDate >= from && a.CreatedDate <= to)
                .OrderByDescending(a => a.CreatedDate)
                .ToListAsync();
        }

        /// <summary>
        /// Lấy audit logs theo user
        /// </summary>
        public async Task<List<AuditLog>> GetUserAuditLogsAsync(int userId)
        {
            return await _context.AuditLogs
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.CreatedDate)
                .ToListAsync();
        }

        /// <summary>
        /// Lấy audit logs theo command type
        /// </summary>
        public async Task<List<AuditLog>> GetAuditLogsByCommandTypeAsync(string commandType)
        {
            return await _context.AuditLogs
                .Where(a => a.CommandType == commandType)
                .OrderByDescending(a => a.CreatedDate)
                .ToListAsync();
        }

        /// <summary>
        /// Lấy failed commands
        /// </summary>
        public async Task<List<AuditLog>> GetFailedCommandsAsync()
        {
            return await _context.AuditLogs
                .Where(a => !a.Success)
                .OrderByDescending(a => a.CreatedDate)
                .ToListAsync();
        }

        /// <summary>
        /// Thống kê lệnh theo loại
        /// </summary>
        public async Task<Dictionary<string, int>> GetCommandStatisticsAsync(DateTime from, DateTime to)
        {
            return await _context.AuditLogs
                .Where(a => a.CreatedDate >= from && a.CreatedDate <= to)
                .GroupBy(a => a.CommandType)
                .ToDictionaryAsync(g => g.Key, g => g.Count());
        }

        /// <summary>
        /// Lấy các người dùng hoạt động nhất
        /// </summary>
        public async Task<Dictionary<string, int>> GetMostActiveUsersAsync(DateTime from, DateTime to, int topCount = 10)
        {
            return await _context.AuditLogs
                .Where(a => a.CreatedDate >= from && a.CreatedDate <= to && a.Username != null)
                .GroupBy(a => a.Username)
                .OrderByDescending(g => g.Count())
                .Take(topCount)
                .ToDictionaryAsync(g => g.Key, g => g.Count());
        }
    }
}
