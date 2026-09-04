namespace LibraryApi.DTOs.Auth;

public class SendChangePasswordCodeRequest
{
    public string CurrentPassword { get; set; } = string.Empty;
}