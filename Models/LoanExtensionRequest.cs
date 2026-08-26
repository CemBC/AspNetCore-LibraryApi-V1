using LibraryApi.Models.Status;

namespace LibraryApi.Models;

public class LoanExtensionRequest
{
    public int Id { get; set; }


    public int LoanId { get; set; }


    public int MemberId { get; set; }


    public DateTime RequestDate { get; set; }


    public LoanExtensionStatus Status { get; set; }


    public Loan Loan { get; set; } = null!;


    public Member Member { get; set; } = null!;
}