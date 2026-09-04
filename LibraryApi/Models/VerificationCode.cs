using LibraryApi.Models.Status;

namespace LibraryApi.Models;

public class VerificationCode
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string CodeHash { get; set; } = string.Empty;

    public VerificationPurpose Purpose { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime ExpiresAt { get; set; }

    public DateTime? UsedAt { get; set; }

    public User User { get; set; } = null!;
}