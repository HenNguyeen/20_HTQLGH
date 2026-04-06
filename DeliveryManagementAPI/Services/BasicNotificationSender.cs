using DeliveryManagementAPI.Models;
using DeliveryManagementAPI.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace DeliveryManagementAPI.Services
{
    /// <summary>
    /// Implementation cơ bản của INotificationSender
    /// Gửi thông báo real-time qua SignalR
    /// </summary>
    public class BasicNotificationSender : INotificationSender
    {
        private readonly IHubContext<NotificationHub> _hubContext;

        public BasicNotificationSender(IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task SendAsync(Notification notification)
        {
            // Gửi đến user cụ thể qua SignalR
            await _hubContext.Clients
                .Group($"user_{notification.UserId}")
                .SendAsync("ReceiveNotification", new
                {
                    id = notification.Id,
                    title = notification.Title,
                    message = notification.Message,
                    type = notification.Type.ToString(),
                    relatedEntityId = notification.RelatedEntityId,
                    actionUrl = notification.ActionUrl,
                    createdAt = notification.CreatedAt,
                    imageUrl = notification.ImageUrl
                });
        }

        public async Task SendToGroupAsync(string groupName, Notification notification)
        {
            // Gửi đến một nhóm users
            await _hubContext.Clients
                .Group(groupName)
                .SendAsync("ReceiveNotification", new
                {
                    id = notification.Id,
                    title = notification.Title,
                    message = notification.Message,
                    type = notification.Type.ToString(),
                    relatedEntityId = notification.RelatedEntityId,
                    actionUrl = notification.ActionUrl,
                    createdAt = notification.CreatedAt,
                    imageUrl = notification.ImageUrl
                });
        }
    }
}
