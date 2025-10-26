using System;
using System.ComponentModel.DataAnnotations;

namespace DeliveryManagementAPI.Models
{
    public class Feedback
    {
        [Key]
        public int FeedbackId { get; set; }
        public int OrderId { get; set; }
        public int UserId { get; set; }
        public int Rating { get; set; } // 1-5 sao
        public string Comment { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
