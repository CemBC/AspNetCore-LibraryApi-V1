namespace LibraryApi.Models
{
    public class Member
    {
        
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        public ICollection<Loan> Loans = new List<Loan>();
    }
}
