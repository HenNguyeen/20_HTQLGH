using DeliveryManagementAPI.Models;
using System.Diagnostics;

namespace DeliveryManagementAPI.Services
{
    /// <summary>
    /// Decorator thêm logging cho notification sending
    /// Design Pattern: Decorator Pattern (Pattern #11)
    /// </summary>
    public class LoggingNotificationDecorator : NotificationSenderDecorator
    {
        private readonly ILogger<LoggingNotificationDecorator> _logger;

        public LoggingNotificationDecorator(
            INotificationSender inner,
            ILogger<LoggingNotificationDecorator> logger) : base(inner)
        {
            _logger = logger;
        }

        public override async Task SendAsync(Notification notification)
        {
            _logger.LogInformation(
                "[Notification] Đang gửi thông báo ID={NotificationId} cho User={UserId}, Type={Type}, Title={Title}",
                notification.Id,
                notification.UserId,
                notification.Type,
                notification.Title);

            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                await base.SendAsync(notification);
                stopwatch.Stop();

                _logger.LogInformation(
                    "[Notification] ✅ Đã gửi thành công thông báo ID={NotificationId} trong {ElapsedMs}ms",
                    notification.Id,
                    stopwatch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                
                _logger.LogError(ex,
                    "[Notification] ❌ Lỗi khi gửi thông báo ID={NotificationId} sau {ElapsedMs}ms",
                    notification.Id,
                    stopwatch.ElapsedMilliseconds);
                
                throw;
            }
        }

        public override async Task SendToGroupAsync(string groupName, Notification notification)
        {
            _logger.LogInformation(
                "[Notification] Đang gửi thông báo ID={NotificationId} đến Group={GroupName}, Type={Type}",
                notification.Id,
                groupName,
                notification.Type);

            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                await base.SendToGroupAsync(groupName, notification);
                stopwatch.Stop();

                _logger.LogInformation(
                    "[Notification] ✅ Đã gửi thành công đến group {GroupName} trong {ElapsedMs}ms",
                    groupName,
                    stopwatch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                
                _logger.LogError(ex,
                    "[Notification] ❌ Lỗi khi gửi đến group {GroupName} sau {ElapsedMs}ms",
                    groupName,
                    stopwatch.ElapsedMilliseconds);
                
                throw;
            }
        }
    }
}
