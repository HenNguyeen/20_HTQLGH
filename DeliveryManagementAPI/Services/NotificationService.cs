using DeliveryManagementAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using DeliveryManagementAPI.Hubs;

namespace DeliveryManagementAPI.Services
{
    /// <summary>
    /// Service quản lý thông báo
    /// </summary>
    public class NotificationService : INotificationService
    {
        private readonly DeliveryDbContext _context;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(
            DeliveryDbContext context,
            IHubContext<NotificationHub> hubContext,
            ILogger<NotificationService> logger)
        {
            _context = context;
            _hubContext = hubContext;
            _logger = logger;
        }

        // ========== In-App Notifications ==========

        public async Task<Notification> CreateNotificationAsync(
            int userId,
            string title,
            string message,
            NotificationType type,
            int? relatedEntityId = null,
            string? actionUrl = null)
        {
            var notification = new Notification
            {
                UserId = userId,
                Title = title,
                Message = message,
                Type = type,
                RelatedEntityId = relatedEntityId,
                ActionUrl = actionUrl,
                RelatedEntityType = type.ToString(),
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Created notification {notification.Id} for user {userId}");

            return notification;
        }

        public async Task<List<NotificationListDto>> GetUserNotificationsAsync(
            int userId,
            int page = 1,
            int pageSize = 20)
        {
            var skip = (page - 1) * pageSize;

            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Skip(skip)
                .Take(pageSize)
                .Select(n => new NotificationListDto
                {
                    Id = n.Id,
                    Title = n.Title,
                    Message = n.Message,
                    Type = n.Type,
                    RelatedEntityType = n.RelatedEntityType,
                    RelatedEntityId = n.RelatedEntityId,
                    IsRead = n.IsRead,
                    CreatedAt = n.CreatedAt,
                    ImageUrl = n.ImageUrl,
                    ActionUrl = n.ActionUrl,
                    TimeAgo = "" // Will be calculated after query
                })
                .ToListAsync();

            // Calculate TimeAgo after query to avoid EF Core translation issue
            foreach (var notification in notifications)
            {
                notification.TimeAgo = GetTimeAgo(notification.CreatedAt);
            }

            return notifications;
        }

        public async Task<int> GetUnreadCountAsync(int userId)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .CountAsync();
        }

        public async Task<bool> MarkAsReadAsync(int notificationId, int userId)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

            if (notification == null)
                return false;

            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Gửi cập nhật real-time
            await _hubContext.Clients.User(userId.ToString())
                .SendAsync("NotificationRead", notificationId);

            return true;
        }

        public async Task<bool> MarkAllAsReadAsync(int userId)
        {
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            foreach (var notification in notifications)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            // Gửi cập nhật real-time
            await _hubContext.Clients.User(userId.ToString())
                .SendAsync("AllNotificationsRead");

            return true;
        }

        public async Task<bool> DeleteNotificationAsync(int notificationId, int userId)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

            if (notification == null)
                return false;

            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<int> DeleteAllReadAsync(int userId)
        {
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId && n.IsRead)
                .ToListAsync();

            _context.Notifications.RemoveRange(notifications);
            await _context.SaveChangesAsync();

            return notifications.Count;
        }

        // ========== Multi-Channel Notifications ==========

        public async Task SendOrderNotificationAsync(int orderId, string eventType)
        {
            try
            {
                var order = await _context.Orders
                    .Include(o => o.Customer)
                    .Include(o => o.AssignedStaff)
                    .FirstOrDefaultAsync(o => o.OrderId == orderId);

                if (order == null)
                {
                    _logger.LogWarning($"Order {orderId} not found for notification");
                    return;
                }

                string title = string.Empty;
                string message = string.Empty;
                string actionUrl = $"/order-detail.html?id={orderId}";

                // Xác định nội dung thông báo
                switch (eventType.ToLower())
                {
                    case "created":
                        title = "Đơn hàng mới";
                        message = $"Đơn hàng #{order.OrderCode} đã được tạo thành công";
                        break;
                    case "confirmed":
                        title = "Đơn hàng đã xác nhận";
                        message = $"Đơn hàng #{order.OrderCode} đã được xác nhận và sẽ sớm được giao";
                        break;
                    case "assigned":
                        title = "Đơn hàng đã được gán";
                        message = $"Đơn hàng #{order.OrderCode} đã được gán cho shipper";
                        break;
                    case "picked_up":
                        title = "Đã lấy hàng";
                        message = $"Shipper đã lấy hàng cho đơn #{order.OrderCode}";
                        break;
                    case "in_transit":
                        title = "Đang giao hàng";
                        message = $"Đơn hàng #{order.OrderCode} đang trên đường giao đến bạn";
                        break;
                    case "delivered":
                        title = "Giao hàng thành công";
                        message = $"Đơn hàng #{order.OrderCode} đã được giao thành công";
                        break;
                    case "failed":
                        title = "Giao hàng thất bại";
                        message = $"Đơn hàng #{order.OrderCode} giao không thành công";
                        break;
                    case "cancelled":
                        title = "Đơn hàng đã hủy";
                        message = $"Đơn hàng #{order.OrderCode} đã bị hủy";
                        break;
                    default:
                        title = "Cập nhật đơn hàng";
                        message = $"Đơn hàng #{order.OrderCode} có cập nhật mới";
                        break;
                }

                // Gửi thông báo cho khách hàng (CreatedByUserId)
                if (order.CreatedByUserId.HasValue)
                {
                    await SendNotificationToUserAsync(
                        order.CreatedByUserId.Value,
                        title,
                        message,
                        NotificationType.Order,
                        orderId,
                        actionUrl);
                }

                // Gửi thông báo cho shipper nếu đã được assign
                if (order.AssignedStaff != null && eventType.ToLower() == "assigned")
                {
                    // Tìm UserAccount của shipper qua FullName
                    var shipperUser = await _context.UserAccounts
                        .FirstOrDefaultAsync(u => u.Role == "shipper" && u.FullName == order.AssignedStaff.FullName);
                    
                    if (shipperUser != null)
                    {
                        await SendNotificationToUserAsync(
                            shipperUser.UserId,
                            "Đơn hàng mới",
                            $"Bạn có đơn hàng mới #{order.OrderCode} cần giao",
                            NotificationType.Order,
                            orderId,
                            actionUrl);
                    }
                }

                _logger.LogInformation($"Sent order notification for order {orderId}, event: {eventType}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending order notification for order {orderId}");
            }
        }

        public async Task SendChatNotificationAsync(int messageId, int recipientUserId)
        {
            try
            {
                var message = await _context.ChatMessages
                    .Include(m => m.Sender)
                    .FirstOrDefaultAsync(m => m.Id == messageId);

                if (message == null)
                {
                    _logger.LogWarning($"Chat message {messageId} not found for notification");
                    return;
                }

                var senderName = message.Sender?.FullName ?? "Ng\u01b0\u1eddi d\u00f9ng";
                var messagePreview = message.Content?.Length > 50 
                    ? message.Content.Substring(0, 50) + "..." 
                    : message.Content ?? "[H\u00ecnh \u1ea3nh]";

                await SendNotificationToUserAsync(
                    recipientUserId,
                    "Tin nh\u1eafn m\u1edbi",
                    $"{senderName}: {messagePreview}",
                    NotificationType.Chat,
                    message.OrderId,
                    $"/messages.html?orderId={message.OrderId}");

                _logger.LogInformation($"Sent chat notification for message {messageId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending chat notification for message {messageId}");
            }
        }

        public async Task SendFeedbackNotificationAsync(int feedbackId, int recipientUserId)
        {
            try
            {
                var feedback = await _context.Feedbacks
                    .FirstOrDefaultAsync(f => f.FeedbackId == feedbackId);

                if (feedback == null)
                {
                    _logger.LogWarning($"Feedback {feedbackId} not found for notification");
                    return;
                }
                
                // Lấy thông tin order
                var order = await _context.Orders
                    .FirstOrDefaultAsync(o => o.OrderId == feedback.OrderId);

                await SendNotificationToUserAsync(
                    recipientUserId,
                    "Đánh giá mới",
                    $"Bạn nhận được đánh giá {feedback.Rating} sao cho đơn hàng #{order?.OrderCode ?? feedback.OrderId.ToString()}",
                    NotificationType.Feedback,
                    feedback.OrderId,
                    $"/order-detail.html?id={feedback.OrderId}");

                _logger.LogInformation($"Sent feedback notification for feedback {feedbackId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending feedback notification for feedback {feedbackId}");
            }
        }

        // ========== Notification Settings ==========

        public async Task<NotificationSetting> GetUserSettingsAsync(int userId)
        {
            var settings = await _context.NotificationSettings
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (settings == null)
            {
                // Tạo settings mặc định nếu chưa có
                settings = await CreateDefaultSettingsAsync(userId);
            }

            return settings;
        }

        public async Task<NotificationSetting> UpdateUserSettingsAsync(int userId, NotificationSetting settings)
        {
            var existingSettings = await _context.NotificationSettings
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (existingSettings == null)
            {
                existingSettings = new NotificationSetting { UserId = userId };
                _context.NotificationSettings.Add(existingSettings);
            }

            existingSettings.EmailEnabled = settings.EmailEnabled;
            existingSettings.SmsEnabled = settings.SmsEnabled;
            existingSettings.InAppEnabled = settings.InAppEnabled;
            existingSettings.PushEnabled = settings.PushEnabled;
            existingSettings.OrderNotifications = settings.OrderNotifications;
            existingSettings.ChatNotifications = settings.ChatNotifications;
            existingSettings.FeedbackNotifications = settings.FeedbackNotifications;
            existingSettings.PromotionNotifications = settings.PromotionNotifications;
            existingSettings.SystemNotifications = settings.SystemNotifications;
            existingSettings.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return existingSettings;
        }

        public async Task<NotificationSetting> CreateDefaultSettingsAsync(int userId)
        {
            var settings = new NotificationSetting
            {
                UserId = userId,
                EmailEnabled = true,
                SmsEnabled = false,
                InAppEnabled = true,
                PushEnabled = true,
                OrderNotifications = true,
                ChatNotifications = true,
                FeedbackNotifications = true,
                PromotionNotifications = true,
                SystemNotifications = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.NotificationSettings.Add(settings);
            await _context.SaveChangesAsync();

            return settings;
        }

        // ========== Private Helper Methods ==========

        private async Task SendNotificationToUserAsync(
            int userId,
            string title,
            string message,
            NotificationType type,
            int? relatedEntityId,
            string? actionUrl)
        {
            // Kiểm tra cài đặt thông báo
            var settings = await GetUserSettingsAsync(userId);

            if (!settings.InAppEnabled)
                return;

            // Kiểm tra loại thông báo có được bật không
            bool shouldSend = type switch
            {
                NotificationType.Order => settings.OrderNotifications,
                NotificationType.Chat => settings.ChatNotifications,
                NotificationType.Feedback => settings.FeedbackNotifications,
                NotificationType.Promotion => settings.PromotionNotifications,
                NotificationType.System => settings.SystemNotifications,
                _ => true
            };

            if (!shouldSend)
                return;

            // Tạo notification trong database
            var notification = await CreateNotificationAsync(
                userId, title, message, type, relatedEntityId, actionUrl);

            // Gửi real-time qua SignalR
            try
            {
                var notificationDto = new NotificationListDto
                {
                    Id = notification.Id,
                    Title = notification.Title,
                    Message = notification.Message,
                    Type = notification.Type,
                    RelatedEntityType = notification.RelatedEntityType,
                    RelatedEntityId = notification.RelatedEntityId,
                    IsRead = notification.IsRead,
                    CreatedAt = notification.CreatedAt,
                    ImageUrl = notification.ImageUrl,
                    ActionUrl = notification.ActionUrl,
                    TimeAgo = GetTimeAgo(notification.CreatedAt)
                };

                await _hubContext.Clients.User(userId.ToString())
                    .SendAsync("ReceiveNotification", notificationDto);

                // Gửi update badge count
                var unreadCount = await GetUnreadCountAsync(userId);
                await _hubContext.Clients.User(userId.ToString())
                    .SendAsync("UpdateUnreadCount", unreadCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending real-time notification to user {userId}");
            }
        }

        private static string GetTimeAgo(DateTime dateTime)
        {
            var timeSpan = DateTime.UtcNow - dateTime;

            if (timeSpan.TotalMinutes < 1)
                return "Vừa xong";
            if (timeSpan.TotalMinutes < 60)
                return $"{(int)timeSpan.TotalMinutes} phút trước";
            if (timeSpan.TotalHours < 24)
                return $"{(int)timeSpan.TotalHours} giờ trước";
            if (timeSpan.TotalDays < 7)
                return $"{(int)timeSpan.TotalDays} ngày trước";
            if (timeSpan.TotalDays < 30)
                return $"{(int)(timeSpan.TotalDays / 7)} tuần trước";
            if (timeSpan.TotalDays < 365)
                return $"{(int)(timeSpan.TotalDays / 30)} tháng trước";

            return $"{(int)(timeSpan.TotalDays / 365)} năm trước";
        }
    }
}
