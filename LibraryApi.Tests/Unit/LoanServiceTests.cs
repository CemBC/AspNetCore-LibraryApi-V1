using LibraryApi.Data;
using LibraryApi.DTOs.Common;
using LibraryApi.DTOs.Loans;
using LibraryApi.Exceptions;
using LibraryApi.Helpers;
using LibraryApi.Models;
using LibraryApi.Models.Status;
using LibraryApi.Services;
using LibraryApi.Tests.Helpers;

namespace LibraryApi.Tests.Unit;

public class LoanServiceTests
{
    [Fact]
    public async Task CreateLoan_CreatesActiveLoanAndMarksBookLoaned()
    {
        await using LibraryDbContext context =
            TestServiceFactory.CreateContext();

        LoanService service =
            TestServiceFactory.CreateLoanService(context);

        (_, Member member) =
            await TestServiceFactory.AddMemberAsync(context);

        Book book =
            TestServiceFactory.AddBook(context);

        DateTime before =
            DateTime.UtcNow;

        Loan loan =
            await service.CreateLoan(
                new CreateLoanRequest
                {
                    BookId = book.Id,
                    MemberId = member.Id
                },
                UserId: 1);

        Assert.Equal(
            LoanStatus.Active,
            loan.Status);

        Assert.Null(
            loan.ReturnDate);

        Assert.True(
            loan.LoanDate >= before);

        Assert.InRange(
            loan.DueDate,
            before.AddDays(7).AddSeconds(-2),
            DateTime.UtcNow.AddDays(7).AddSeconds(2));

        Book? dbBook =
            await context.Books.FindAsync(book.Id);

        Assert.NotNull(dbBook);

        Assert.Equal(
            BookStatus.Loaned,
            dbBook.Status);
    }


    [Fact]
    public async Task CreateLoan_WhenBookDoesNotExist_ThrowsNotFoundException()
    {
        await using LibraryDbContext context =
            TestServiceFactory.CreateContext();

        LoanService service =
            TestServiceFactory.CreateLoanService(context);

        (_, Member member) =
            await TestServiceFactory.AddMemberAsync(context);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.CreateLoan(
                new CreateLoanRequest
                {
                    BookId = 999,
                    MemberId = member.Id
                },
                UserId: 1));
    }


    [Fact]
    public async Task CreateLoan_WhenMemberDoesNotExist_ThrowsNotFoundException()
    {
        await using LibraryDbContext context =
            TestServiceFactory.CreateContext();

        LoanService service =
            TestServiceFactory.CreateLoanService(context);

        Book book =
            TestServiceFactory.AddBook(context);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.CreateLoan(
                new CreateLoanRequest
                {
                    BookId = book.Id,
                    MemberId = 999
                },
                UserId: 1));
    }


    [Fact]
    public async Task CreateLoan_WhenBookIsNotAvailable_ThrowsBadRequestException()
    {
        await using LibraryDbContext context =
            TestServiceFactory.CreateContext();

        LoanService service =
            TestServiceFactory.CreateLoanService(context);

        (_, Member member) =
            await TestServiceFactory.AddMemberAsync(context);

        Book book =
            TestServiceFactory.AddBook(
                context,
                status: BookStatus.Requested);

        await Assert.ThrowsAsync<BadRequestException>(() =>
            service.CreateLoan(
                new CreateLoanRequest
                {
                    BookId = book.Id,
                    MemberId = member.Id
                },
                UserId: 1));
    }


    [Fact]
    public async Task CreateLoan_WhenMemberHasOverdueLoan_ThrowsBadRequestException()
    {
        await using LibraryDbContext context =
            TestServiceFactory.CreateContext();

        LoanService service =
            TestServiceFactory.CreateLoanService(context);

        (_, Member member) =
            await TestServiceFactory.AddMemberAsync(context);

        Book overdueBook =
            TestServiceFactory.AddBook(
                context,
                "Overdue Book",
                status: BookStatus.Loaned);

        Book requestedBook =
            TestServiceFactory.AddBook(
                context,
                "New Book");

        TestServiceFactory.AddLoan(
            context,
            overdueBook,
            member,
            LoanStatus.Active,
            dueDate: DateTime.UtcNow.AddDays(-1));

        await Assert.ThrowsAsync<BadRequestException>(() =>
            service.CreateLoan(
                new CreateLoanRequest
                {
                    BookId = requestedBook.Id,
                    MemberId = member.Id
                },
                UserId: 1));
    }


    [Fact]
    public async Task CreateLoan_WhenMemberHasThreeUnreturnedLoans_ThrowsBadRequestException()
    {
        await using LibraryDbContext context =
            TestServiceFactory.CreateContext();

        LoanService service =
            TestServiceFactory.CreateLoanService(context);

        (_, Member member) =
            await TestServiceFactory.AddMemberAsync(context);

        for (int i = 0;
             i < LibraryRules.MaxActiveLoansPerMember;
             i++)
        {
            Book existingBook =
                TestServiceFactory.AddBook(
                    context,
                    $"Existing {i}",
                    status: BookStatus.Loaned);

            TestServiceFactory.AddLoan(
                context,
                existingBook,
                member,
                LoanStatus.Active,
                dueDate: DateTime.UtcNow.AddDays(5 + i));
        }

        Book newBook =
            TestServiceFactory.AddBook(
                context,
                "Fourth Book");

        await Assert.ThrowsAsync<BadRequestException>(() =>
            service.CreateLoan(
                new CreateLoanRequest
                {
                    BookId = newBook.Id,
                    MemberId = member.Id
                },
                UserId: 1));
    }


    [Fact]
    public async Task ReturnBook_MarksLoanReturnedAndBookAvailable()
    {
        await using LibraryDbContext context =
            TestServiceFactory.CreateContext();

        LoanService service =
            TestServiceFactory.CreateLoanService(context);

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

        await service.ReturnBook(
            loan.Id,
            UserId: 1);

        Loan? dbLoan =
            await context.Loans.FindAsync(loan.Id);

        Book? dbBook =
            await context.Books.FindAsync(book.Id);

        Assert.NotNull(dbLoan);
        Assert.NotNull(dbBook);

        Assert.Equal(
            LoanStatus.Returned,
            dbLoan.Status);

        Assert.NotNull(
            dbLoan.ReturnDate);

        Assert.Equal(
            BookStatus.Available,
            dbBook.Status);
    }


    [Fact]
    public async Task ReturnBook_WhenAlreadyReturned_ThrowsBadRequestException()
    {
        await using LibraryDbContext context =
            TestServiceFactory.CreateContext();

        LoanService service =
            TestServiceFactory.CreateLoanService(context);

        (_, Member member) =
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
            service.ReturnBook(
                loan.Id,
                UserId: 1));
    }


    [Fact]
    public async Task UpdateOverdueLoansAsync_ChangesOnlyExpiredActiveLoans()
    {
        await using LibraryDbContext context =
            TestServiceFactory.CreateContext();

        LoanService service =
            TestServiceFactory.CreateLoanService(context);

        (_, Member member) =
            await TestServiceFactory.AddMemberAsync(context);

        Book overdueBook =
            TestServiceFactory.AddBook(
                context,
                "Overdue",
                status: BookStatus.Loaned);

        Book futureBook =
            TestServiceFactory.AddBook(
                context,
                "Future",
                status: BookStatus.Loaned);

        Book returnedBook =
            TestServiceFactory.AddBook(
                context,
                "Returned");

        Loan overdue =
            TestServiceFactory.AddLoan(
                context,
                overdueBook,
                member,
                LoanStatus.Active,
                dueDate: DateTime.UtcNow.AddDays(-2));

        Loan future =
            TestServiceFactory.AddLoan(
                context,
                futureBook,
                member,
                LoanStatus.Active,
                dueDate: DateTime.UtcNow.AddDays(2));

        Loan returned =
            TestServiceFactory.AddLoan(
                context,
                returnedBook,
                member,
                LoanStatus.Returned,
                dueDate: DateTime.UtcNow.AddDays(-2),
                returnDate: DateTime.UtcNow.AddDays(-1));

        await service.UpdateOverdueLoansAsync();

        Loan? dbOverdue =
            await context.Loans.FindAsync(overdue.Id);

        Loan? dbFuture =
            await context.Loans.FindAsync(future.Id);

        Loan? dbReturned =
            await context.Loans.FindAsync(returned.Id);

        Assert.NotNull(dbOverdue);
        Assert.NotNull(dbFuture);
        Assert.NotNull(dbReturned);

        Assert.Equal(
            LoanStatus.Overdue,
            dbOverdue.Status);

        Assert.Equal(
            LoanStatus.Active,
            dbFuture.Status);

        Assert.Equal(
            LoanStatus.Returned,
            dbReturned.Status);
    }


    [Fact]
    public async Task GetMyLoansAsync_ReturnsOnlyAuthenticatedMembersLoans()
    {
        await using LibraryDbContext context =
            TestServiceFactory.CreateContext();

        LoanService service =
            TestServiceFactory.CreateLoanService(context);

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
                "Mine");

        Book book2 =
            TestServiceFactory.AddBook(
                context,
                "Other");

        TestServiceFactory.AddLoan(
            context,
            book1,
            member1);

        TestServiceFactory.AddLoan(
            context,
            book2,
            member2);

        List<LoanResponse> result =
            await service.GetMyLoansAsync(user1.Id);

        Assert.Single(result);

        Assert.Equal(
            member1.Id,
            result[0].MemberId);

        Assert.Equal(
            "Mine",
            result[0].BookTitle);
    }


    [Fact]
    public async Task GetAllAsync_AppliesSearchStatusSortAndPagination()
    {
        await using LibraryDbContext context =
            TestServiceFactory.CreateContext();

        LoanService service =
            TestServiceFactory.CreateLoanService(context);

        (_, Member member) =
            await TestServiceFactory.AddMemberAsync(
                context,
                fullName: "Cem Test");

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

        TestServiceFactory.AddLoan(
            context,
            b1,
            member,
            LoanStatus.Active,
            DateTime.UtcNow.AddDays(3));

        TestServiceFactory.AddLoan(
            context,
            b2,
            member,
            LoanStatus.Active,
            DateTime.UtcNow.AddDays(1));

        TestServiceFactory.AddLoan(
            context,
            b3,
            member,
            LoanStatus.Returned,
            DateTime.UtcNow.AddDays(4),
            DateTime.UtcNow);

        PagedResponse<LoanResponse> result =
            await service.GetAllAsync(
                new LoanQuery
                {
                    Page = 1,
                    PageSize = 1,
                    Search = "Cem",
                    Status = LoanStatus.Active,
                    SortBy = "dueDate",
                    Descending = false
                });

        Assert.Equal(
            2,
            result.TotalCount);

        Assert.Equal(
            2,
            result.TotalPages);

        Assert.Single(
            result.Items);

        Assert.Equal(
            "Beta",
            result.Items[0].BookTitle);
    }


    [Fact]
    public async Task GetActiveLoansAsync_ReturnsOnlyActiveLoans()
    {
        await using LibraryDbContext context =
            TestServiceFactory.CreateContext();

        LoanService service =
            TestServiceFactory.CreateLoanService(context);

        (_, Member member) =
            await TestServiceFactory.AddMemberAsync(context);

        Book activeBook =
            TestServiceFactory.AddBook(
                context,
                "Active",
                status: BookStatus.Loaned);

        Book overdueBook =
            TestServiceFactory.AddBook(
                context,
                "Overdue",
                status: BookStatus.Loaned);

        TestServiceFactory.AddLoan(
            context,
            activeBook,
            member,
            LoanStatus.Active);

        TestServiceFactory.AddLoan(
            context,
            overdueBook,
            member,
            LoanStatus.Overdue,
            DateTime.UtcNow.AddDays(-2));

        PagedResponse<LoanResponse> result =
            await service.GetActiveLoansAsync(
                new LoanQuery());

        Assert.Single(
            result.Items);

        Assert.Equal(
            LoanStatus.Active,
            result.Items[0].Status);
    }


    [Fact]
    public async Task GetOverdueLoansAsync_ReturnsOnlyOverdueLoans()
    {
        await using LibraryDbContext context =
            TestServiceFactory.CreateContext();

        LoanService service =
            TestServiceFactory.CreateLoanService(context);

        (_, Member member) =
            await TestServiceFactory.AddMemberAsync(context);

        Book activeBook =
            TestServiceFactory.AddBook(
                context,
                "Active",
                status: BookStatus.Loaned);

        Book overdueBook =
            TestServiceFactory.AddBook(
                context,
                "Overdue",
                status: BookStatus.Loaned);

        TestServiceFactory.AddLoan(
            context,
            activeBook,
            member,
            LoanStatus.Active);

        TestServiceFactory.AddLoan(
            context,
            overdueBook,
            member,
            LoanStatus.Overdue,
            DateTime.UtcNow.AddDays(-2));

        PagedResponse<LoanResponse> result =
            await service.GetOverdueLoansAsync(
                new LoanQuery());

        Assert.Single(
            result.Items);

        Assert.Equal(
            LoanStatus.Overdue,
            result.Items[0].Status);
    }
}