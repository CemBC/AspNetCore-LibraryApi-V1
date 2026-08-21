namespace LibraryApi.DTOs.Members;

public class MemberResponse
{
    public int Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;
}