namespace LibraryApi.DTOs.Members;

public class UpdateMemberRequest
{
    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;
}