using SQLite;

[Table("Users")]
public class UserModels
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = "Owner";

    // API/Cloud Integration
    public string? CloudUserId { get; set; } // The ID from your Laravel/Web API
    public string? LastLoginToken { get; set; }
    // Subscription Details
    public bool IsSubscribed { get; set; } = false;
    public DateTime? SubscriptionExpiry { get; set; }
    public string SubscriptionTier { get; set; } = "Free"; // e.g., Basic, Pro, Premium
    public bool IsStaffGateEnabled { get; set; } = false;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime LastSyncAt { get; set; } = DateTime.Now;
}