namespace LibraryApi.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = "Member";

        public string? RefreshTokenHash { get; set; }

        public DateTime? RefreshTokenExpiryTime { get; set; }
    }
}
