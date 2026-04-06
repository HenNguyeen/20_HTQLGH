using DeliveryManagementAPI.Models;

namespace DeliveryManagementAPI.Services
{
    /// <summary>
    /// Base Decorator cho INotificationSender
    /// Triển khai Decorator Pattern để mở rộng functionality
    /// </summary>
    public abstract class NotificationSenderDecorator : INotificationSender
    {
        protected readonly INotificationSender _inner;

        protected NotificationSenderDecorator(INotificationSender inner)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        }

        public virtual async Task SendAsync(Notification notification)
        {
            await _inner.SendAsync(notification);
        }

        public virtual async Task SendToGroupAsync(string groupName, Notification notification)
        {
            await _inner.SendToGroupAsync(groupName, notification);
        }
    }
}
