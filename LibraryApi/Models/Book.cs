using LibraryApi.Models.Status;

namespace LibraryApi.Models;

public class Book
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public string Author { get; set; } = null!;

    public string Description { get; set; } = string.Empty;

    public string? ImageUrl { get; set; }

    public BookStatus Status { get; set; } = BookStatus.Available;

    public ICollection<Loan> Loans { get; set; } = new List<Loan>();

    public ICollection<LoanRequest> LoanRequests { get; set; } = new List<LoanRequest>();
}