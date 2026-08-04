namespace LibraryApi.Models
{
    public enum LoanOperationStatus
    {
        Success,
        BookNotFound,
        MemberNotFound,
        BookUnavailable,
        LoanNotFound,
        AlreadyReturned
    }
}
