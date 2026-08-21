namespace LibraryApi.DTOs.Books;

public class UpdateBookRequest
{
    public string Title { get; set; } = string.Empty;

    public string Author { get; set; } = string.Empty;
}