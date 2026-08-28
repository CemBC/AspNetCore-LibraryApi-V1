namespace LibraryApi.DTOs.Loans
{
    public class CreateLoanRequest
    {
        public int BookId { get; set; }
        public int MemberId { get; set; }
    }
}