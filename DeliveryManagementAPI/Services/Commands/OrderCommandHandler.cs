using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace DeliveryManagementAPI.Services.Commands
{
    /// <summary>
    /// Handler để thực thi các Order commands (Command Pattern #12)
    /// Đảm bảo validation, logging, audit trail tracking
    /// </summary>
    public class OrderCommandHandler
    {
        private readonly OrderService _orderService;
        private readonly AuditLogService _auditLogService;
        private readonly ILogger<OrderCommandHandler> _logger;
        private readonly List<IOrderCommand> _commandHistory;
        
        // Thông tin context (user, IP, etc.) để lưu vào audit log
        private int? _currentUserId;
        private string? _currentUsername;
        private string? _currentUserRole;
        private string? _currentIPAddress;

        public OrderCommandHandler(
            OrderService orderService, 
            AuditLogService auditLogService,
            ILogger<OrderCommandHandler> logger)
        {
            _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
            _auditLogService = auditLogService ?? throw new ArgumentNullException(nameof(auditLogService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _commandHistory = new List<IOrderCommand>();
        }

        /// <summary>
        /// Set user context cho audit logging
        /// </summary>
        public void SetUserContext(int userId, string username, string userRole, string? ipAddress = null)
        {
            _currentUserId = userId;
            _currentUsername = username;
            _currentUserRole = userRole;
            _currentIPAddress = ipAddress;
        }

        /// <summary>
        /// Thực thi một command với audit logging
        /// </summary>
        public async Task ExecuteAsync(IOrderCommand command)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            var stopwatch = Stopwatch.StartNew();
            bool success = false;
            string? errorMessage = null;

            try
            {
                _logger.LogInformation($"[Command] Executing: {command.GetDescription()} | Type: {command.CommandType}");

                // Thực thi command
                await command.ExecuteAsync(_orderService);

                // Lưu vào command history
                _commandHistory.Add(command);

                success = true;
                _logger.LogInformation($"[Command] Success: {command.GetDescription()}");
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                _logger.LogError($"[Command] Failed: {command.GetDescription()} | Error: {ex.Message}");
                throw;
            }
            finally
            {
                stopwatch.Stop();

                // Lưu audit log vào database
                await _auditLogService.LogCommandAsync(
                    commandType: command.CommandType.ToString(),
                    commandDescription: command.GetDescription(),
                    orderId: ExtractOrderIdFromCommand(command),
                    orderCode: ExtractOrderCodeFromCommand(command),
                    oldValue: null,
                    newValue: null,
                    userId: _currentUserId,
                    username: _currentUsername,
                    userRole: _currentUserRole,
                    ipAddress: _currentIPAddress,
                    success: success,
                    errorMessage: errorMessage,
                    executionTimeMs: stopwatch.ElapsedMilliseconds
                );
            }
        }

        /// <summary>
        /// Thực thi nhiều commands (transaction)
        /// </summary>
        public async Task ExecuteMultipleAsync(params IOrderCommand[] commands)
        {
            if (commands == null || commands.Length == 0)
                throw new ArgumentException("Phải cung cấp ít nhất một command", nameof(commands));

            foreach (var command in commands)
            {
                await ExecuteAsync(command);
            }
        }

        /// <summary>
        /// Lấy lịch sử các commands đã thực thi
        /// </summary>
        public IReadOnlyList<IOrderCommand> GetCommandHistory()
        {
            return _commandHistory.AsReadOnly();
        }

        /// <summary>
        /// Xóa lịch sử commands
        /// </summary>
        public void ClearCommandHistory()
        {
            _commandHistory.Clear();
        }

        /// <summary>
        /// Lấy số lượng commands đã thực thi
        /// </summary>
        public int GetCommandHistoryCount()
        {
            return _commandHistory.Count;
        }

        /// <summary>
        /// Extract Order ID từ command (nếu có)
        /// </summary>
        private int? ExtractOrderIdFromCommand(IOrderCommand command)
        {
            // Có thể extend logic này để lấy OrderId từ các command khác nhau
            return null; // TODO: Implement khi cần
        }

        /// <summary>
        /// Extract Order Code từ command (nếu có)
        /// </summary>
        private string? ExtractOrderCodeFromCommand(IOrderCommand command)
        {
            // Có thể extend logic này để lấy OrderCode từ các command khác nhau
            return null; // TODO: Implement khi cần
        }
    }
}
