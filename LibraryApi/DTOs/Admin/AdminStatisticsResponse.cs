namespace LibraryApi.DTOs.Admin;

public class AdminStatisticsResponse
{
    public int TotalBooks { get; set; }
    public int AvailableBooks { get; set; }
    public int TotalMembers { get; set; }
    public int ActiveLoans { get; set; }
    public int OverdueLoans { get; set; }
    public int PendingLoanRequests { get; set; }
    public int PendingExtensionRequests { get; set; }
}