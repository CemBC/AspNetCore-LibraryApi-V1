using LibraryApi.Models;
using LibraryApi.Models.Status;

namespace LibraryApi.DTOs.Loans;

public class LoanResponse
{
    public int Id { get; set; }


    public int BookId { get; set; }

    public string BookTitle { get; set; } = null!;

    public int MemberId { get; set; }

    public string MemberName { get; set; } = null!;


    public DateTime LoanDate { get; set; }


    public DateTime DueDate { get; set; }


    public DateTime? ReturnDate { get; set; }


    public LoanStatus Status { get; set; }
}