namespace DeliveryManagementAPI.Models
{
    /// <summary>
    /// Audit Log Model - Ghi lại tất cả các hành động trên Order (Command Pattern)
    /// Dùng cho tracking, compliance, troubleshooting
    /// </summary>
    public class AuditLog
    {
        public int AuditLogId { get; set; }
        
        // Thông tin hành động
        public string CommandType { get; set; } = string.Empty; // Create, Update, Delete, UpdateStatus, etc.
        public string CommandDescription { get; set; } = string.Empty; // Chi tiết hành động
        
        // Thông tin liên quan
        public int? OrderId { get; set; } // Order ID nếu liên quan
        public string? OrderCode { get; set; } // Mã đơn hàng
        
        // Dữ liệu
        public string? OldValue { get; set; } // JSON - dữ liệu trước (cho comparison)
        public string? NewValue { get; set; } // JSON - dữ liệu sau
        
        // Metadata
        public int? UserId { get; set; } // Người thực hiện
        public string? Username { get; set; } // Tên user
        public string? UserRole { get; set; } // Vai trò: admin, staff, customer
        public string? IPAddress { get; set; } // IP address của user
        
        // Thời gian
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        
        // Status
        public bool Success { get; set; } = true; // Hành động thành công hay thất bại?
        public string? ErrorMessage { get; set; } // Lỗi nếu có
        
        // Performance
        public long ExecutionTimeMs { get; set; } // Thời gian thực thi (milliseconds)
    }
}
