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

        [MaxLength(2000)]
        public string? Content { get; set; }

        [MaxLength(500)]
        public string? ImageUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        public Order? Order { get; set; }
        public UserAccount? Sender { get; set; }
    }
}
