namespace DeliveryManagementAPI.Models
{
    /// <summary>
    /// DTO để tạo thông báo
    /// </summary>
    public class CreateNotificationDto
    {
        public int UserId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public NotificationType Type { get; set; }
        public int? RelatedEntityId { get; set; }
        public string? ActionUrl { get; set; }
    }
    
    /// <summary>
    /// DTO để trả về danh sách thông báo
    /// </summary>
    public class NotificationListDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public NotificationType Type { get; set; }
        public string? RelatedEntityType { get; set; }
        public int? RelatedEntityId { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? ImageUrl { get; set; }
        public string? ActionUrl { get; set; }
        public string TimeAgo { get; set; } = string.Empty;
    }
    
    /// <summary>
    /// DTO để mark as read
    /// </summary>
    public class MarkAsReadDto
    {
        public List<int> NotificationIds { get; set; } = new List<int>();
    }
}
