namespace LibraryApi.DTOs.Auth;

public class CurrentUserResponse
{
    public int Id { get; set; }


    public string Email { get; set; } = null!;


    public string Role { get; set; } = null!;



    public int? MemberId { get; set; }


    public string? FullName { get; set; }
}