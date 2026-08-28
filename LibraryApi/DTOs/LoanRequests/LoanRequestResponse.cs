using LibraryApi.Models.Status;

namespace LibraryApi.DTOs.LoanRequests;

public class LoanRequestResponse
{
    public int Id { get; set; }


    public string LoanCode { get; set; } = null!;


    public int BookId { get; set; }


    public string BookTitle { get; set; } = null!;


    public int MemberId { get; set; }


    public string MemberName { get; set; } = null!;


    public DateTime RequestDate { get; set; }


    public LoanRequestStatus Status { get; set; }
}