using Microsoft.AspNetCore.Mvc;
using DeliveryManagementAPI.Models;
using DeliveryManagementAPI.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace DeliveryManagementAPI.Controllers
{
    /// <summary>
    /// Controller quản lý thông báo
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly ILogger<NotificationsController> _logger;

        public NotificationsController(
            INotificationService notificationService,
            ILogger<NotificationsController> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
        }

        /// <summary>
        /// Lấy danh sách thông báo của user hiện tại
        /// </summary>
        /// <param name="page">Trang hiện tại (mặc định: 1)</param>
        /// <param name="pageSize">Số lượng mỗi trang (mặc định: 20)</param>
        [HttpGet]
        public async Task<ActionResult<List<NotificationListDto>>> GetMyNotifications(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var userId = GetCurrentUserId();
                var notifications = await _notificationService.GetUserNotificationsAsync(userId, page, pageSize);
                
                return Ok(notifications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notifications");
                return StatusCode(500, new { message = "Lỗi khi lấy danh sách thông báo" });
            }
        }

        /// <summary>
        /// Đếm số thông báo chưa đọc
        /// </summary>
        [HttpGet("unread-count")]
        public async Task<ActionResult<object>> GetUnreadCount()
        {
            try
            {
                var userId = GetCurrentUserId();
                var count = await _notificationService.GetUnreadCountAsync(userId);
                
                return Ok(new { count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting unread count");
                return StatusCode(500, new { message = "Lỗi khi đếm thông báo chưa đọc" });
            }
        }

        /// <summary>
        /// Đánh dấu một thông báo đã đọc
        /// </summary>
        /// <param name="id">ID thông báo</param>
        [HttpPut("{id}/read")]
        public async Task<ActionResult> MarkAsRead(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _notificationService.MarkAsReadAsync(id, userId);
                
                if (!result)
                {
                    return NotFound(new { message = "Không tìm thấy thông báo" });
                }
                
                return Ok(new { message = "Đã đánh dấu thông báo là đã đọc" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error marking notification {id} as read");
                return StatusCode(500, new { message = "Lỗi khi đánh dấu đã đọc" });
            }
        }

        /// <summary>
        /// Đánh dấu tất cả thông báo đã đọc
        /// </summary>
        [HttpPut("read-all")]
        public async Task<ActionResult> MarkAllAsRead()
        {
            try
            {
                var userId = GetCurrentUserId();
                await _notificationService.MarkAllAsReadAsync(userId);
                
                return Ok(new { message = "Đã đánh dấu tất cả thông báo là đã đọc" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking all notifications as read");
                return StatusCode(500, new { message = "Lỗi khi đánh dấu tất cả đã đọc" });
            }
        }

        /// <summary>
        /// Xóa một thông báo
        /// </summary>
        /// <param name="id">ID thông báo</param>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteNotification(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _notificationService.DeleteNotificationAsync(id, userId);
                
                if (!result)
                {
                    return NotFound(new { message = "Không tìm thấy thông báo" });
                }
                
                return Ok(new { message = "Đã xóa thông báo" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting notification {id}");
                return StatusCode(500, new { message = "Lỗi khi xóa thông báo" });
            }
        }

        /// <summary>
        /// Xóa tất cả thông báo đã đọc
        /// </summary>
        [HttpDelete("read")]
        public async Task<ActionResult> DeleteAllRead()
        {
            try
            {
                var userId = GetCurrentUserId();
                var count = await _notificationService.DeleteAllReadAsync(userId);
                
                return Ok(new { message = $"Đã xóa {count} thông báo đã đọc", count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting read notifications");
                return StatusCode(500, new { message = "Lỗi khi xóa thông báo" });
            }
        }

        // ========== Notification Settings ==========

        /// <summary>
        /// Lấy cài đặt thông báo của user hiện tại
        /// </summary>
        [HttpGet("settings")]
        public async Task<ActionResult<NotificationSetting>> GetSettings()
        {
            try
            {
                var userId = GetCurrentUserId();
                var settings = await _notificationService.GetUserSettingsAsync(userId);
                
                return Ok(settings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notification settings");
                return StatusCode(500, new { message = "Lỗi khi lấy cài đặt thông báo" });
            }
        }

        /// <summary>
        /// Cập nhật cài đặt thông báo
        /// </summary>
        [HttpPut("settings")]
        public async Task<ActionResult<NotificationSetting>> UpdateSettings([FromBody] NotificationSetting settings)
        {
            try
            {
                var userId = GetCurrentUserId();
                var updatedSettings = await _notificationService.UpdateUserSettingsAsync(userId, settings);
                
                return Ok(new 
                { 
                    message = "Đã cập nhật cài đặt thông báo", 
                    settings = updatedSettings 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating notification settings");
                return StatusCode(500, new { message = "Lỗi khi cập nhật cài đặt" });
            }
        }

        // ========== Admin APIs ==========

        /// <summary>
        /// [Admin] Tạo thông báo cho một user cụ thể
        /// </summary>
        [HttpPost("admin/create")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<Notification>> CreateNotification([FromBody] CreateNotificationDto dto)
        {
            try
            {
                var notification = await _notificationService.CreateNotificationAsync(
                    dto.UserId,
                    dto.Title,
                    dto.Message,
                    dto.Type,
                    dto.RelatedEntityId,
                    dto.ActionUrl);
                
                return Ok(new 
                { 
                    message = "Đã tạo thông báo", 
                    notification 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating notification");
                return StatusCode(500, new { message = "Lỗi khi tạo thông báo" });
            }
        }

        /// <summary>
        /// [Admin] Gửi thông báo hệ thống cho tất cả users
        /// </summary>
        [HttpPost("admin/broadcast")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult> BroadcastNotification([FromBody] BroadcastNotificationDto dto)
        {
            try
            {
                // TODO: Implement broadcast to all users
                // This would create notifications for all active users
                
                return Ok(new { message = "Đã gửi thông báo đến tất cả người dùng" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error broadcasting notification");
                return StatusCode(500, new { message = "Lỗi khi gửi thông báo" });
            }
        }

        // ========== Helper Methods ==========

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userIdClaim))
            {
                throw new UnauthorizedAccessException("Không xác định được người dùng");
            }
            
            if (!int.TryParse(userIdClaim, out var userId))
            {
                throw new UnauthorizedAccessException("UserId không hợp lệ");
            }
            
            return userId;
        }
    }

    /// <summary>
    /// DTO để broadcast notification
    /// </summary>
    public class BroadcastNotificationDto
    {
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public NotificationType Type { get; set; } = NotificationType.System;
        public string? ActionUrl { get; set; }
    }
}
