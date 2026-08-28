using LibraryApi.Models.Status;

namespace LibraryApi.DTOs.LoanExtensions;

public class LoanExtensionResponse
{
    public int Id { get; set; }


    public int LoanId { get; set; }


    public string BookTitle { get; set; } = null!;


    public int MemberId { get; set; }


    public DateTime RequestDate { get; set; }


    public LoanExtensionStatus Status { get; set; }
}