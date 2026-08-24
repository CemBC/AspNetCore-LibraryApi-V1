namespace LibraryApi.DTOs.Auth;

public class CurrentUserResponse
{
    public int Id { get; set; }

    public string Email { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;
}