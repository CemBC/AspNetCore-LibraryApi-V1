using LibraryApi.Models.Status;

namespace LibraryApi.DTOs.Books;

public class BookResponse
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Author { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string? ImageUrl { get; set; }

    public BookStatus Status { get; set; }
}