using System.ComponentModel.DataAnnotations;

namespace DeliveryManagementAPI.Models
{
    /// <summary>
    /// Cài đặt thông báo của người dùng
    /// </summary>
    public class NotificationSetting
    {
        [Key]
        public int Id { get; set; }
        
        /// <summary>
        /// ID người dùng
        /// </summary>
        public int UserId { get; set; }
        
        /// <summary>
        /// Bật thông báo Email
        /// </summary>
        public bool EmailEnabled { get; set; } = true;
        
        /// <summary>
        /// Bật thông báo SMS
        /// </summary>
        public bool SmsEnabled { get; set; } = false;
        
        /// <summary>
        /// Bật thông báo trong app
        /// </summary>
        public bool InAppEnabled { get; set; } = true;
        
        /// <summary>
        /// Bật push notification
        /// </summary>
        public bool PushEnabled { get; set; } = true;
        
        // Chi tiết cho từng loại thông báo
        
        /// <summary>
        /// Nhận thông báo về đơn hàng
        /// </summary>
        public bool OrderNotifications { get; set; } = true;
        
        /// <summary>
        /// Nhận thông báo về chat
        /// </summary>
        public bool ChatNotifications { get; set; } = true;
        
        /// <summary>
        /// Nhận thông báo về feedback
        /// </summary>
        public bool FeedbackNotifications { get; set; } = true;
        
        /// <summary>
        /// Nhận thông báo về khuyến mãi
        /// </summary>
        public bool PromotionNotifications { get; set; } = true;
        
        /// <summary>
        /// Nhận thông báo hệ thống
        /// </summary>
        public bool SystemNotifications { get; set; } = true;
        
        /// <summary>
        /// Ngày tạo
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// Ngày cập nhật
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
