using DeliveryManagementAPI.Models;

namespace DeliveryManagementAPI.Services
{
    /// <summary>
    /// Interface cho Notification Service
    /// </summary>
    public interface INotificationService
    {
        // ========== In-App Notifications ==========
        
        /// <summary>
        /// Tạo thông báo mới trong database
        /// </summary>
        Task<Notification> CreateNotificationAsync(
            int userId, 
            string title, 
            string message, 
            NotificationType type, 
            int? relatedEntityId = null,
            string? actionUrl = null);
        
        /// <summary>
        /// Lấy danh sách thông báo của user với phân trang
        /// </summary>
        Task<List<NotificationListDto>> GetUserNotificationsAsync(
            int userId, 
            int page = 1, 
            int pageSize = 20);
        
        /// <summary>
        /// Đếm số thông báo chưa đọc
        /// </summary>
        Task<int> GetUnreadCountAsync(int userId);
        
        /// <summary>
        /// Đánh dấu thông báo đã đọc
        /// </summary>
        Task<bool> MarkAsReadAsync(int notificationId, int userId);
        
        /// <summary>
        /// Đánh dấu tất cả thông báo đã đọc
        /// </summary>
        Task<bool> MarkAllAsReadAsync(int userId);
        
        /// <summary>
        /// Xóa thông báo
        /// </summary>
        Task<bool> DeleteNotificationAsync(int notificationId, int userId);
        
        /// <summary>
        /// Xóa tất cả thông báo đã đọc
        /// </summary>
        Task<int> DeleteAllReadAsync(int userId);
        
        // ========== Multi-Channel Notifications ==========
        
        /// <summary>
        /// Gửi thông báo về đơn hàng (multi-channel)
        /// </summary>
        Task SendOrderNotificationAsync(int orderId, string eventType);
        
        /// <summary>
        /// Gửi thông báo về chat message
        /// </summary>
        Task SendChatNotificationAsync(int messageId, int recipientUserId);
        
        /// <summary>
        /// Gửi thông báo về feedback mới
        /// </summary>
        Task SendFeedbackNotificationAsync(int feedbackId, int recipientUserId);
        
        // ========== Notification Settings ==========
        
        /// <summary>
        /// Lấy cài đặt thông báo của user
        /// </summary>
        Task<NotificationSetting> GetUserSettingsAsync(int userId);
        
        /// <summary>
        /// Cập nhật cài đặt thông báo
        /// </summary>
        Task<NotificationSetting> UpdateUserSettingsAsync(int userId, NotificationSetting settings);
        
        /// <summary>
        /// Tạo cài đặt mặc định cho user mới
        /// </summary>
        Task<NotificationSetting> CreateDefaultSettingsAsync(int userId);
    }
}
