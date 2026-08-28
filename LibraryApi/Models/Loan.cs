using LibraryApi.Models;
using LibraryApi.Models.Status;

public class Loan
{
    public int Id { get; set; }

    public int BookId { get; set; }

    public int MemberId { get; set; }

    public DateTime LoanDate { get; set; }

    public DateTime DueDate { get; set; }

    public DateTime? ReturnDate { get; set; }

    public LoanStatus Status { get; set; }
        = LoanStatus.Active;


    public Book Book { get; set; } = null!;

    public Member Member { get; set; } = null!;

    public ICollection<LoanExtensionRequest> ExtensionRequests { get; set; }
    = new List<LoanExtensionRequest>();
}