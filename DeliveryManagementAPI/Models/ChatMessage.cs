using System;
using System.ComponentModel.DataAnnotations;

namespace DeliveryManagementAPI.Models
{
    public class ChatMessage
    {
        [Key]
        public int Id { get; set; }

        public int? OrderId { get; set; }

        [Required]
        public int SenderId { get; set; }

        [Required]
        [MaxLength(20)]
        public string SenderRole { get; set; } = string.Empty; // "Customer", "Shipper", "Admin"

        // Thêm ReceiverId để tracking người nhận tin nhắn
        // Đặc biệt quan trọng cho general support chat (OrderId = null)
        // Khi khách hàng nhắn tin -> ReceiverId = null (gửi cho admin)
        // Khi admin reply -> ReceiverId = userId của khách hàng cụ thể
        public int? ReceiverId { get; set; }

        [MaxLength(2000)]
        public string? Content { get; set; }

        [MaxLength(500)]
        public string? ImageUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        public Order? Order { get; set; }
        public UserAccount? Sender { get; set; }
        public UserAccount? Receiver { get; set; }
    }
}
