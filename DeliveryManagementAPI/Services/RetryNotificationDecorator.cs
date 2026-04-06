using DeliveryManagementAPI.Models;

namespace DeliveryManagementAPI.Services
{
    /// <summary>
    /// Decorator thêm retry logic cho notification sending
    /// Tự động retry khi gửi thất bại
    /// Design Pattern: Decorator Pattern (Pattern #11)
    /// </summary>
    public class RetryNotificationDecorator : NotificationSenderDecorator
    {
        private readonly int _maxRetries;
        private readonly int _delayMs;
        private readonly ILogger<RetryNotificationDecorator> _logger;

        public RetryNotificationDecorator(
            INotificationSender inner,
            ILogger<RetryNotificationDecorator> logger,
            int maxRetries = 3,
            int delayMs = 1000) : base(inner)
        {
            _maxRetries = maxRetries;
            _delayMs = delayMs;
            _logger = logger;
        }

        public override async Task SendAsync(Notification notification)
        {
            int attemptCount = 0;
            Exception? lastException = null;

            while (attemptCount < _maxRetries)
            {
                try
                {
                    attemptCount++;
                    
                    if (attemptCount > 1)
                    {
                        _logger.LogWarning(
                            "[Retry] Thử lại lần {Attempt}/{MaxRetries} gửi notification ID={NotificationId}",
                            attemptCount,
                            _maxRetries,
                            notification.Id);
                    }

                    await base.SendAsync(notification);
                    
                    // Thành công
                    if (attemptCount > 1)
                    {
                        _logger.LogInformation(
                            "[Retry] ✅ Thành công sau {Attempt} lần thử",
                            attemptCount);
                    }
                    
                    return;
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    
                    if (attemptCount < _maxRetries)
                    {
                        _logger.LogWarning(
                            "[Retry] ⚠️ Lần thử {Attempt} thất bại, chờ {Delay}ms trước khi thử lại...",
                            attemptCount,
                            _delayMs);
                        
                        await Task.Delay(_delayMs);
                    }
                }
            }

            // Thất bại sau tất cả các lần thử
            _logger.LogError(
                lastException,
                "[Retry] ❌ Gửi thông báo thất bại sau {MaxRetries} lần thử",
                _maxRetries);
            
            throw new Exception(
                $"Gửi thông báo ID={notification.Id} thất bại sau {_maxRetries} lần thử",
                lastException);
        }

        public override async Task SendToGroupAsync(string groupName, Notification notification)
        {
            int attemptCount = 0;
            Exception? lastException = null;

            while (attemptCount < _maxRetries)
            {
                try
                {
                    attemptCount++;
                    
                    if (attemptCount > 1)
                    {
                        _logger.LogWarning(
                            "[Retry] Thử lại lần {Attempt}/{MaxRetries} gửi đến group {GroupName}",
                            attemptCount,
                            _maxRetries,
                            groupName);
                    }

                    await base.SendToGroupAsync(groupName, notification);
                    
                    if (attemptCount > 1)
                    {
                        _logger.LogInformation(
                            "[Retry] ✅ Thành công sau {Attempt} lần thử",
                            attemptCount);
                    }
                    
                    return;
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    
                    if (attemptCount < _maxRetries)
                    {
                        _logger.LogWarning(
                            "[Retry] ⚠️ Lần thử {Attempt} thất bại, chờ {Delay}ms trước khi thử lại...",
                            attemptCount,
                            _delayMs);
                        
                        await Task.Delay(_delayMs);
                    }
                }
            }

            _logger.LogError(
                lastException,
                "[Retry] ❌ Gửi đến group {GroupName} thất bại sau {MaxRetries} lần thử",
                groupName,
                _maxRetries);
            
            throw new Exception(
                $"Gửi đến group {groupName} thất bại sau {_maxRetries} lần thử",
                lastException);
        }
    }
}
