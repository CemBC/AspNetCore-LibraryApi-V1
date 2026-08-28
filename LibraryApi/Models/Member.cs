using LibraryApi.Models;

public class Member
{
    public int Id { get; set; }


    public int UserId { get; set; }


    public string FullName { get; set; } = null!;


    public DateTime CreatedAt { get; set; }



    public User User { get; set; } = null!;



    public ICollection<Loan> Loans { get; set; }
        = new List<Loan>();

    public ICollection<LoanRequest> LoanRequests { get; set; }
    = new List<LoanRequest>();

    public ICollection<LoanExtensionRequest> ExtensionRequests { get; set; }
    = new List<LoanExtensionRequest>();
}