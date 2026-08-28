using LibraryApi.Models.Status;

namespace LibraryApi.Models;

public class LoanRequest
{
    public int Id { get; set; }


    public int BookId { get; set; }


    public int MemberId { get; set; }


    public string LoanCode { get; set; } = null!;


    public DateTime RequestDate { get; set; }


    public LoanRequestStatus Status { get; set; }



    public Book Book { get; set; } = null!;


    public Member Member { get; set; } = null!;
}