namespace LibraryApi.DTOs.Auth;

public class ChangeEmailRequest
{
    public string NewEmail { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;
}