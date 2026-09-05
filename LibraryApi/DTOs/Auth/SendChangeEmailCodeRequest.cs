namespace LibraryApi.DTOs.Auth;

public class SendChangeEmailCodeRequest
{
    public string NewEmail { get; set; } = string.Empty;
}