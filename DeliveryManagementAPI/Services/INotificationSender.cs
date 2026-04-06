using DeliveryManagementAPI.Models;

namespace DeliveryManagementAPI.Services
{
    /// <summary>
    /// Interface cho việc gửi thông báo real-time
    /// Sử dụng Decorator Pattern để mở rộng chức năng (logging, retry, etc.)
    /// </summary>
    public interface INotificationSender
    {
        /// <summary>
        /// Gửi thông báo real-time đến user qua SignalR
        /// </summary>
        Task SendAsync(Notification notification);

        /// <summary>
        /// Gửi thông báo đến một nhóm users
        /// </summary>
        Task SendToGroupAsync(string groupName, Notification notification);
    }
}
