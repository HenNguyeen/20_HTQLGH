using System.Text.Json.Serialization;

namespace DeliveryManagementAPI.Models
{
    public class UserAccount
    {
        [System.ComponentModel.DataAnnotations.Key]
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty; // "admin", "shipper", "customer"
    // Google account id (sub) if user signed up via Google
    public string? GoogleId { get; set; }
        [JsonIgnore]
        public string? PasswordResetToken { get; set; }
        [JsonIgnore]
        public DateTime? ResetTokenExpiry { get; set; }
    }
}