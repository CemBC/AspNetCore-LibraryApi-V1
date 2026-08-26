namespace LibraryApi.DTOs.Auth;

public class RegisterRequest
{
    public string Email { get; set; } = null!;


    public string Password { get; set; } = null!;


    public string FullName { get; set; } = null!;
}