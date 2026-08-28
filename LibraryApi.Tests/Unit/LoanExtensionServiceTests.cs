using LibraryApi.Data;
using LibraryApi.DTOs.Common;
using LibraryApi.DTOs.LoanExtensions;
using LibraryApi.Exceptions;
using LibraryApi.Helpers;
using LibraryApi.Models;
using LibraryApi.Models.Status;
using LibraryApi.Services;
using LibraryApi.Tests.Helpers;

namespace LibraryApi.Tests.Unit;

public class LoanExtensionServiceTests
{
    [Fact]
    public async Task CreateAsync_CreatesPendingExtensionForOwnedActiveLoan()
    {
        await using LibraryDbContext context =
            TestServiceFactory.CreateContext();

        LoanExtensionService service =
            TestServiceFactory.CreateLoanExtensionService(context);

        (User user, Member member) =
            await TestServiceFactory.AddMemberAsync(context);

        Book book =
            TestServiceFactory.AddBook(
                context,
                status: BookStatus.Loaned);

        Loan loan =
            TestServiceFactory.AddLoan(
                context,
                book,
                member,
                LoanStatus.Active,
                DateTime.UtcNow.AddDays(3));

        LoanExtensionResponse response =
            await service.CreateAsync(
                user.Id,
                new CreateLoanExtensionRequestDto
                {
                    LoanId = loan.Id
                });

        LoanExtensionRequest? request =
            await context.LoanExtensionRequest.FindAsync(response.Id);

        Assert.NotNull(request);

        Assert.Equal(
            LoanExtensionStatus.Pending,
            request.Status);

        Assert.Equal(
            loan.Id,
            request.LoanId);

        Assert.Equal(
            member.Id,
            request.MemberId);

        Assert.Equal(
            request.Id,
            response.Id);
    }


    [Fact]
    public async Task CreateAsync_WhenLoanBelongsToAnotherMember_ThrowsBadRequestException()
    {
        await using LibraryDbContext context =
            TestServiceFactory.CreateContext();

        LoanExtensionService service =
            TestServiceFactory.CreateLoanExtensionService(context);

        (User user1, _) =
            await TestServiceFactory.AddMemberAsync(
                context,
                "one@test.com",
                "One");

        (_, Member member2) =
            await TestServiceFactory.AddMemberAsync(
                context,
                "two@test.com",
                "Two");

        Book book =
            TestServiceFactory.AddBook(
                context,
                status: BookStatus.Loaned);

        Loan loan =
            TestServiceFactory.AddLoan(
                context,
                book,
                member2);

        await Assert.ThrowsAsync<BadRequestException>(() =>
            service.CreateAsync(
                user1.Id,
                new CreateLoanExtensionRequestDto
                {
                    LoanId = loan.Id
                }));
    }


    [Fact]
    public async Task CreateAsync_WhenLoanIsNotActive_ThrowsBadRequestException()
    {
        await using LibraryDbContext context =
            TestServiceFactory.CreateContext();

        LoanExtensionService service =
            TestServiceFactory.CreateLoanExtensionService(context);

        (User user, Member member) =
            await TestServiceFactory.AddMemberAsync(context);

        Book book =
            TestServiceFactory.AddBook(context);

        Loan loan =
            TestServiceFactory.AddLoan(
                context,
                book,
                member,
                LoanStatus.Returned,
                returnDate: DateTime.UtcNow);

        await Assert.ThrowsAsync<BadRequestException>(() =>
            service.CreateAsync(
                user.Id,
                new CreateLoanExtensionRequestDto
                {
                    LoanId = loan.Id
                }));
    }


    [Fact]
    public async Task CreateAsync_WhenDueDatePassedEvenIfStatusActive_ThrowsBadRequestException()
    {
        await using LibraryDbContext context =
            TestServiceFactory.CreateContext();

        LoanExtensionService service =
            TestServiceFactory.CreateLoanExtensionService(context);

        (User user, Member member) =
            await TestServiceFactory.AddMemberAsync(context);

        Book book =
            TestServiceFactory.AddBook(
                context,
                status: BookStatus.Loaned);

        Loan loan =
            TestServiceFactory.AddLoan(
                context,
                book,
                member,
                LoanStatus.Active,
                dueDate: DateTime.UtcNow.AddMinutes(-1));

        await Assert.ThrowsAsync<BadRequestException>(() =>
            service.CreateAsync(
                user.Id,
                new CreateLoanExtensionRequestDto
                {
                    LoanId = loan.Id
                }));
    }


    [Fact]
    public async Task CreateAsync_WhenPendingExtensionAlreadyExists_ThrowsBadRequestException()
    {
        await using LibraryDbContext context =
            TestServiceFactory.CreateContext();

        LoanExtensionService service =
            TestServiceFactory.CreateLoanExtensionService(context);

        (User user, Member member) =
            await TestServiceFactory.AddMemberAsync(context);

        Book book =
            TestServiceFactory.AddBook(
                context,
                status: BookStatus.Loaned);

        Loan loan =
            TestServiceFactory.AddLoan(
                context,
                book,
                member);

        TestServiceFactory.AddExtensionRequest(
            context,
            loan,
            member,
            LoanExtensionStatus.Pending);

        await Assert.ThrowsAsync<BadRequestException>(() =>
            service.CreateAsync(
                user.Id,
                new CreateLoanExtensionRequestDto
                {
                    LoanId = loan.Id
                }));
    }


    [Fact]
    public async Task CreateAsync_WhenMaximumApprovedExtensionsReached_ThrowsBadRequestException()
    {
        await using LibraryDbContext context =
            TestServiceFactory.CreateContext();

        LoanExtensionService service =
            TestServiceFactory.CreateLoanExtensionService(context);

        (User user, Member member) =
            await TestServiceFactory.AddMemberAsync(context);

        Book book =
            TestServiceFactory.AddBook(
                context,
                status: BookStatus.Loaned);

        Loan loan =
            TestServiceFactory.AddLoan(
                context,
                book,
                member);

        for (int i = 0;
             i < LibraryRules.MaxApprovedExtensionsPerLoan;
             i++)
        {
            TestServiceFactory.AddExtensionRequest(
                context,
                loan,
                member,
                LoanExtensionStatus.Approved,
                DateTime.UtcNow.AddDays(-i - 1));
        }

        await Assert.ThrowsAsync<BadRequestException>(() =>
            service.CreateAsync(
                user.Id,
                new CreateLoanExtensionRequestDto
                {
                    LoanId = loan.Id
                }));
    }


    [Fact]
    public async Task ApproveAsync_MarksApprovedAndAddsSevenDaysToDueDate()
    {
        await using LibraryDbContext context =
            TestServiceFactory.CreateContext();

        LoanExtensionService service =
            TestServiceFactory.CreateLoanExtensionService(context);

        (_, Member member) =
            await TestServiceFactory.AddMemberAsync(context);

        Book book =
            TestServiceFactory.AddBook(
                context,
                status: BookStatus.Loaned);

        DateTime originalDueDate =
            DateTime.UtcNow.AddDays(3);

        Loan loan =
            TestServiceFactory.AddLoan(
                context,
                book,
                member,
                LoanStatus.Active,
                originalDueDate);

        LoanExtensionRequest request =
            TestServiceFactory.AddExtensionRequest(
                context,
                loan,
                member);

        LoanExtensionResponse response =
            await service.ApproveAsync(
                request.Id,
                UserId: 1);

        Loan? dbLoan =
            await context.Loans.FindAsync(loan.Id);

        LoanExtensionRequest? dbRequest =
            await context.LoanExtensionRequest.FindAsync(request.Id);

        Assert.NotNull(dbLoan);
        Assert.NotNull(dbRequest);

        Assert.Equal(
            LoanExtensionStatus.Approved,
            dbRequest.Status);

        Assert.Equal(
            originalDueDate.AddDays(7),
            dbLoan.DueDate);

        Assert.Equal(
            LoanExtensionStatus.Approved,
            response.Status);
    }


    [Fact]
    public async Task ApproveAsync_WhenMaximumApprovedExtensionsReached_ThrowsBadRequestException()
    {
        await using LibraryDbContext context =
            TestServiceFactory.CreateContext();

        LoanExtensionService service =
            TestServiceFactory.CreateLoanExtensionService(context);

        (_, Member member) =
            await TestServiceFactory.AddMemberAsync(context);

        Book book =
            TestServiceFactory.AddBook(
                context,
                status: BookStatus.Loaned);

        Loan loan =
            TestServiceFactory.AddLoan(
                context,
                book,
                member);

        for (int i = 0;
             i < LibraryRules.MaxApprovedExtensionsPerLoan;
             i++)
        {
            TestServiceFactory.AddExtensionRequest(
                context,
                loan,
                member,
                LoanExtensionStatus.Approved,
                DateTime.UtcNow.AddDays(-i - 2));
        }

        LoanExtensionRequest pending =
            TestServiceFactory.AddExtensionRequest(
                context,
                loan,
                member,
                LoanExtensionStatus.Pending);

        await Assert.ThrowsAsync<BadRequestException>(() =>
            service.ApproveAsync(
                pending.Id,
                UserId: 1));
    }


    [Fact]
    public async Task RejectAsync_MarksRequestRejected()
    {
        await using LibraryDbContext context =
            TestServiceFactory.CreateContext();

        LoanExtensionService service =
            TestServiceFactory.CreateLoanExtensionService(context);

        (_, Member member) =
            await TestServiceFactory.AddMemberAsync(context);

        Book book =
            TestServiceFactory.AddBook(
                context,
                status: BookStatus.Loaned);

        Loan loan =
            TestServiceFactory.AddLoan(
                context,
                book,
                member);

        LoanExtensionRequest request =
            TestServiceFactory.AddExtensionRequest(
                context,
                loan,
                member);

        LoanExtensionResponse response =
            await service.RejectAsync(
                request.Id,
                UserId: 1);

        LoanExtensionRequest? dbRequest =
            await context.LoanExtensionRequest.FindAsync(request.Id);

        Assert.NotNull(dbRequest);

        Assert.Equal(
            LoanExtensionStatus.Rejected,
            dbRequest.Status);

        Assert.Equal(
            LoanExtensionStatus.Rejected,
            response.Status);
    }


    [Fact]
    public async Task GetMyPendingRequestsAsync_ReturnsOnlyOwnedPendingRequests()
    {
        await using LibraryDbContext context =
            TestServiceFactory.CreateContext();

        LoanExtensionService service =
            TestServiceFactory.CreateLoanExtensionService(context);

        (User user1, Member member1) =
            await TestServiceFactory.AddMemberAsync(
                context,
                "one@test.com",
                "One");

        (_, Member member2) =
            await TestServiceFactory.AddMemberAsync(
                context,
                "two@test.com",
                "Two");

        Book book1 =
            TestServiceFactory.AddBook(
                context,
                "Mine",
                status: BookStatus.Loaned);

        Book book2 =
            TestServiceFactory.AddBook(
                context,
                "Other",
                status: BookStatus.Loaned);

        Loan loan1 =
            TestServiceFactory.AddLoan(
                context,
                book1,
                member1);

        Loan loan2 =
            TestServiceFactory.AddLoan(
                context,
                book2,
                member2);

        TestServiceFactory.AddExtensionRequest(
            context,
            loan1,
            member1,
            LoanExtensionStatus.Pending);

        TestServiceFactory.AddExtensionRequest(
            context,
            loan2,
            member2,
            LoanExtensionStatus.Pending);

        List<LoanExtensionResponse> result =
            await service.GetMyPendingRequestsAsync(user1.Id);

        Assert.Single(result);

        Assert.Equal(
            loan1.Id,
            result[0].LoanId);
    }


    [Fact]
    public async Task GetPendingRequestsAsync_AppliesSearchSortAndPagination()
    {
        await using LibraryDbContext context =
            TestServiceFactory.CreateContext();

        LoanExtensionService service =
            TestServiceFactory.CreateLoanExtensionService(context);

        (_, Member member) =
            await TestServiceFactory.AddMemberAsync(
                context,
                fullName: "Cem Member");

        Book b1 =
            TestServiceFactory.AddBook(
                context,
                "Alpha",
                status: BookStatus.Loaned);

        Book b2 =
            TestServiceFactory.AddBook(
                context,
                "Beta",
                status: BookStatus.Loaned);

        Book b3 =
            TestServiceFactory.AddBook(
                context,
                "Gamma",
                status: BookStatus.Loaned);

        Loan l1 =
            TestServiceFactory.AddLoan(
                context,
                b1,
                member);

        Loan l2 =
            TestServiceFactory.AddLoan(
                context,
                b2,
                member);

        Loan l3 =
            TestServiceFactory.AddLoan(
                context,
                b3,
                member);

        TestServiceFactory.AddExtensionRequest(
            context,
            l1,
            member,
            LoanExtensionStatus.Pending,
            DateTime.UtcNow.AddMinutes(-3));

        TestServiceFactory.AddExtensionRequest(
            context,
            l2,
            member,
            LoanExtensionStatus.Pending,
            DateTime.UtcNow.AddMinutes(-2));

        TestServiceFactory.AddExtensionRequest(
            context,
            l3,
            member,
            LoanExtensionStatus.Approved,
            DateTime.UtcNow.AddMinutes(-1));

        PagedResponse<LoanExtensionResponse> result =
            await service.GetPendingRequestsAsync(
                new LoanExtensionRequestQuery
                {
                    Page = 1,
                    PageSize = 1,
                    Search = "Cem",
                    SortBy = "bookTitle",
                    Descending = true
                });

        Assert.Equal(
            2,
            result.TotalCount);

        Assert.Equal(
            2,
            result.TotalPages);

        Assert.Single(result.Items);

        Assert.Equal(
            "Beta",
            result.Items[0].BookTitle);
    }
}