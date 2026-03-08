using System.ComponentModel.DataAnnotations;

namespace DeliveryManagementAPI.Models
{
    /// <summary>
    /// Thông báo trong hệ thống
    /// </summary>
    public class Notification
    {
        [Key]
        public int Id { get; set; }
        
        /// <summary>
        /// ID người nhận thông báo
        /// </summary>
        public int UserId { get; set; }
        
        /// <summary>
        /// Tiêu đề thông báo
        /// </summary>
        public string Title { get; set; } = string.Empty;
        
        /// <summary>
        /// Nội dung thông báo
        /// </summary>
        public string Message { get; set; } = string.Empty;
        
        /// <summary>
        /// Loại thông báo
        /// </summary>
        public NotificationType Type { get; set; }
        
        /// <summary>
        /// Loại entity liên quan (Order, Message, etc.)
        /// </summary>
        public string? RelatedEntityType { get; set; }
        
        /// <summary>
        /// ID của entity liên quan
        /// </summary>
        public int? RelatedEntityId { get; set; }
        
        /// <summary>
        /// Đã đọc chưa
        /// </summary>
        public bool IsRead { get; set; } = false;
        
        /// <summary>
        /// Thời gian tạo
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// Thời gian đọc
        /// </summary>
        public DateTime? ReadAt { get; set; }
        
        /// <summary>
        /// URL hình ảnh đính kèm
        /// </summary>
        public string? ImageUrl { get; set; }
        
        /// <summary>
        /// URL action (link đến trang chi tiết)
        /// </summary>
        public string? ActionUrl { get; set; }
        
        /// <summary>
        /// Dữ liệu bổ sung (JSON)
        /// </summary>
        public string? Data { get; set; }
    }
    
    /// <summary>
    /// Loại thông báo
    /// </summary>
    public enum NotificationType
    {
        /// <summary>
        /// Thông báo về đơn hàng
        /// </summary>
        Order = 1,
        
        /// <summary>
        /// Thông báo về chat/message
        /// </summary>
        Chat = 2,
        
        /// <summary>
        /// Thông báo về tài khoản
        /// </summary>
        Account = 3,
        
        /// <summary>
        /// Thông báo về feedback/đánh giá
        /// </summary>
        Feedback = 4,
        
        /// <summary>
        /// Thông báo về khuyến mãi
        /// </summary>
        Promotion = 5,
        
        /// <summary>
        /// Thông báo hệ thống
        /// </summary>
        System = 6
    }
}
