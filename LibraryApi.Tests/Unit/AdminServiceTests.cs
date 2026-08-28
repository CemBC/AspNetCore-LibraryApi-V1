using LibraryApi.Data;
using LibraryApi.DTOs.Admin;
using LibraryApi.Models;
using LibraryApi.Models.Status;
using LibraryApi.Services;
using LibraryApi.Tests.Helpers;

namespace LibraryApi.Tests.Unit;

public class AdminServiceTests
{
    [Fact]
    public async Task GetStatisticsAsync_ReturnsCorrectCounts()
    {
        await using LibraryDbContext context = TestServiceFactory.CreateContext();

        (_, Member member1) = await TestServiceFactory.AddMemberAsync(context, "one@test.com", "One");
        (_, Member member2) = await TestServiceFactory.AddMemberAsync(context, "two@test.com", "Two");

        Book available = TestServiceFactory.AddBook(context, "Available", status: BookStatus.Available);
        Book activeBook = TestServiceFactory.AddBook(context, "Active", status: BookStatus.Loaned);
        Book overdueBook = TestServiceFactory.AddBook(context, "Overdue", status: BookStatus.Loaned);
        Book requested = TestServiceFactory.AddBook(context, "Requested", status: BookStatus.Requested);

        Loan active = TestServiceFactory.AddLoan(context, activeBook, member1, LoanStatus.Active);
        TestServiceFactory.AddLoan(context, overdueBook, member2, LoanStatus.Overdue, DateTime.UtcNow.AddDays(-2));

        TestServiceFactory.AddLoanRequest(context, requested, member1, LoanRequestStatus.Pending);
        TestServiceFactory.AddExtensionRequest(context, active, member1, LoanExtensionStatus.Pending);

        AdminService service = new(context);

        AdminStatisticsResponse result = await service.GetStatisticsAsync();

        Assert.Equal(4, result.TotalBooks);
        Assert.Equal(1, result.AvailableBooks);
        Assert.Equal(2, result.TotalMembers);
        Assert.Equal(1, result.ActiveLoans);
        Assert.Equal(1, result.OverdueLoans);
        Assert.Equal(1, result.PendingLoanRequests);
        Assert.Equal(1, result.PendingExtensionRequests);
    }
}
