using LibraryApi.Data;
using LibraryApi.DTOs.Common;
using LibraryApi.DTOs.LoanRequests;
using LibraryApi.Exceptions;
using LibraryApi.Helpers;
using LibraryApi.Models;
using LibraryApi.Models.Status;
using LibraryApi.Services;
using LibraryApi.Tests.Helpers;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Tests.Unit;

public class LoanRequestServiceTests
{
    [Fact]
    public async Task CreateAsync_CreatesPendingRequestAndMarksBookRequested()
    {
        await using LibraryDbContext context =
            TestServiceFactory.CreateContext();

        LoanRequestService service =
            TestServiceFactory.CreateLoanRequestService(context);

        (User user, Member member) =
            await TestServiceFactory.AddMemberAsync(context);

        Book book =
            TestServiceFactory.AddBook(context);

        LoanRequestResponse response =
            await service.CreateAsync(
                user.Id,
                new CreateLoanRequestDto
                {
                    BookId = book.Id
                });

        LoanRequest? request =
            await context.LoanRequest.FindAsync(response.Id);

        Book? dbBook =
            await context.Books.FindAsync(book.Id);

        Assert.NotNull(request);
        Assert.NotNull(dbBook);

        Assert.Equal(
            LoanRequestStatus.Pending,
            request.Status);

        Assert.Equal(
            member.Id,
            request.MemberId);

        Assert.StartsWith(
            "LR-",
            request.LoanCode);

        Assert.Equal(
            9,
            request.LoanCode.Length);

        Assert.Equal(
            BookStatus.Requested,
            dbBook.Status);

        Assert.Equal(
            request.Id,
            response.Id);
    }


    [Fact]
    public async Task CreateAsync_WhenMemberDoesNotExist_ThrowsNotFoundException()
    {
        await using LibraryDbContext context =
            TestServiceFactory.CreateContext();

        LoanRequestService service =
            TestServiceFactory.CreateLoanRequestService(context);

        Book book =
            TestServiceFactory.AddBook(context);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.CreateAsync(
                999,
                new CreateLoanRequestDto
                {
                    BookId = book.Id
                }));
    }


    [Fact]
    public async Task CreateAsync_WhenMemberHasOverdueLoan_ThrowsBadRequestException()
    {
        await using LibraryDbContext context =
            TestServiceFactory.CreateContext();

        LoanRequestService service =
            TestServiceFactory.CreateLoanRequestService(context);

        (User user, Member member) =
            await TestServiceFactory.AddMemberAsync(context);

        Book overdueBook =
            TestServiceFactory.AddBook(
                context,
                "Overdue",
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
            service.CreateAsync(
                user.Id,
                new CreateLoanRequestDto
                {
                    BookId = requestedBook.Id
                }));
    }


    [Fact]
    public async Task CreateAsync_WhenMemberHasMaximumLoans_ThrowsBadRequestException()
    {
        await using LibraryDbContext context =
            TestServiceFactory.CreateContext();

        LoanRequestService service =
            TestServiceFactory.CreateLoanRequestService(context);

        (User user, Member member) =
            await TestServiceFactory.AddMemberAsync(context);

        for (int i = 0;
             i < LibraryRules.MaxActiveLoansPerMember;
             i++)
        {
            Book borrowed =
                TestServiceFactory.AddBook(
                    context,
                    $"Borrowed {i}",
                    status: BookStatus.Loaned);

            TestServiceFactory.AddLoan(
                context,
                borrowed,
                member,
                LoanStatus.Active,
                dueDate: DateTime.UtcNow.AddDays(i + 2));
        }

        Book requestedBook =
            TestServiceFactory.AddBook(
                context,
                "Fourth");

        await Assert.ThrowsAsync<BadRequestException>(() =>
            service.CreateAsync(
                user.Id,
                new CreateLoanRequestDto
                {
                    BookId = requestedBook.Id
                }));
    }


    [Fact]
    public async Task CreateAsync_WhenBookIsNotAvailable_ThrowsBadRequestException()
    {
        await using LibraryDbContext context =
            TestServiceFactory.CreateContext();

        LoanRequestService service =
            TestServiceFactory.CreateLoanRequestService(context);

        (User user, _) =
            await TestServiceFactory.AddMemberAsync(context);

        Book book =
            TestServiceFactory.AddBook(
                context,
                status: BookStatus.Loaned);

        await Assert.ThrowsAsync<BadRequestException>(() =>
            service.CreateAsync(
                user.Id,
                new CreateLoanRequestDto
                {
                    BookId = book.Id
                }));
    }


    [Fact]
    public async Task CreateAsync_WhenPendingRequestAlreadyExistsForBook_ThrowsBadRequestException()
    {
        await using LibraryDbContext context =
            TestServiceFactory.CreateContext();

        LoanRequestService service =
            TestServiceFactory.CreateLoanRequestService(context);

        (User user1, Member member1) =
            await TestServiceFactory.AddMemberAsync(
                context,
                "one@test.com",
                "One");

        (User user2, _) =
            await TestServiceFactory.AddMemberAsync(
                context,
                "two@test.com",
                "Two");

        Book book =
            TestServiceFactory.AddBook(context);

        TestServiceFactory.AddLoanRequest(
            context,
            book,
            member1);

        // Duplicate request kuralını izole etmek için
        // kitabı tekrar Available yapıyoruz.
        book.Status = BookStatus.Available;

        await context.SaveChangesAsync();

        await Assert.ThrowsAsync<BadRequestException>(() =>
            service.CreateAsync(
                user2.Id,
                new CreateLoanRequestDto
                {
                    BookId = book.Id
                }));
    }


    [Fact]
    public async Task ApproveAsync_CreatesLoanMarksRequestApprovedAndBookLoaned()
    {
        await using LibraryDbContext context =
            TestServiceFactory.CreateContext();

        LoanRequestService service =
            TestServiceFactory.CreateLoanRequestService(context);

        (_, Member member) =
            await TestServiceFactory.AddMemberAsync(context);

        Book book =
            TestServiceFactory.AddBook(
                context,
                status: BookStatus.Requested);

        LoanRequest request =
            TestServiceFactory.AddLoanRequest(
                context,
                book,
                member);

        LoanRequestResponse response =
            await service.ApproveAsync(
                request.Id,
                UserId: 100);

        Loan? loan =
            await context.Loans.FirstOrDefaultAsync(l =>
                l.BookId == book.Id &&
                l.MemberId == member.Id);

        LoanRequest? dbRequest =
            await context.LoanRequest.FindAsync(request.Id);

        Book? dbBook =
            await context.Books.FindAsync(book.Id);

        Assert.NotNull(loan);
        Assert.NotNull(dbRequest);
        Assert.NotNull(dbBook);

        Assert.Equal(
            LoanRequestStatus.Approved,
            dbRequest.Status);

        Assert.Equal(
            BookStatus.Loaned,
            dbBook.Status);

        Assert.Equal(
            member.Id,
            loan.MemberId);

        Assert.Equal(
            book.Id,
            loan.BookId);

        Assert.Equal(
            LoanStatus.Active,
            loan.Status);

        Assert.Equal(
            LoanRequestStatus.Approved,
            response.Status);
    }


    [Fact]
    public async Task ApproveAsync_WhenRequestIsNotPending_ThrowsBadRequestException()
    {
        await using LibraryDbContext context =
            TestServiceFactory.CreateContext();

        LoanRequestService service =
            TestServiceFactory.CreateLoanRequestService(context);

        (_, Member member) =
            await TestServiceFactory.AddMemberAsync(context);

        Book book =
            TestServiceFactory.AddBook(
                context,
                status: BookStatus.Loaned);

        LoanRequest request =
            TestServiceFactory.AddLoanRequest(
                context,
                book,
                member,
                LoanRequestStatus.Approved);

        await Assert.ThrowsAsync<BadRequestException>(() =>
            service.ApproveAsync(
                request.Id,
                UserId: 1));
    }


    [Fact]
    public async Task ApproveAsync_WhenMemberHasOverdueLoan_ThrowsBadRequestException()
    {
        await using LibraryDbContext context =
            TestServiceFactory.CreateContext();

        LoanRequestService service =
            TestServiceFactory.CreateLoanRequestService(context);

        (_, Member member) =
            await TestServiceFactory.AddMemberAsync(context);

        Book requestedBook =
            TestServiceFactory.AddBook(
                context,
                "Requested",
                status: BookStatus.Requested);

        LoanRequest request =
            TestServiceFactory.AddLoanRequest(
                context,
                requestedBook,
                member);

        Book overdueBook =
            TestServiceFactory.AddBook(
                context,
                "Overdue",
                status: BookStatus.Loaned);

        TestServiceFactory.AddLoan(
            context,
            overdueBook,
            member,
            LoanStatus.Active,
            DateTime.UtcNow.AddDays(-1));

        await Assert.ThrowsAsync<BadRequestException>(() =>
            service.ApproveAsync(
                request.Id,
                UserId: 1));
    }


    [Fact]
    public async Task ApproveAsync_WhenMemberReachedMaximumLoans_ThrowsBadRequestException()
    {
        await using LibraryDbContext context =
            TestServiceFactory.CreateContext();

        LoanRequestService service =
            TestServiceFactory.CreateLoanRequestService(context);

        (_, Member member) =
            await TestServiceFactory.AddMemberAsync(context);

        Book requestedBook =
            TestServiceFactory.AddBook(
                context,
                "Requested",
                status: BookStatus.Requested);

        LoanRequest request =
            TestServiceFactory.AddLoanRequest(
                context,
                requestedBook,
                member);

        for (int i = 0;
             i < LibraryRules.MaxActiveLoansPerMember;
             i++)
        {
            Book borrowed =
                TestServiceFactory.AddBook(
                    context,
                    $"Borrowed {i}",
                    status: BookStatus.Loaned);

            TestServiceFactory.AddLoan(
                context,
                borrowed,
                member,
                LoanStatus.Active,
                DateTime.UtcNow.AddDays(5 + i));
        }

        await Assert.ThrowsAsync<BadRequestException>(() =>
            service.ApproveAsync(
                request.Id,
                UserId: 1));
    }


    [Fact]
    public async Task RejectAsync_MarksRequestRejectedAndBookAvailable()
    {
        await using LibraryDbContext context =
            TestServiceFactory.CreateContext();

        LoanRequestService service =
            TestServiceFactory.CreateLoanRequestService(context);

        (_, Member member) =
            await TestServiceFactory.AddMemberAsync(context);

        Book book =
            TestServiceFactory.AddBook(
                context,
                status: BookStatus.Requested);

        LoanRequest request =
            TestServiceFactory.AddLoanRequest(
                context,
                book,
                member);

        LoanRequestResponse response =
            await service.RejectAsync(
                request.Id,
                UserId: 1);

        LoanRequest? dbRequest =
            await context.LoanRequest.FindAsync(request.Id);

        Book? dbBook =
            await context.Books.FindAsync(book.Id);

        Assert.NotNull(dbRequest);
        Assert.NotNull(dbBook);

        Assert.Equal(
            LoanRequestStatus.Rejected,
            response.Status);

        Assert.Equal(
            LoanRequestStatus.Rejected,
            dbRequest.Status);

        Assert.Equal(
            BookStatus.Available,
            dbBook.Status);
    }


    [Fact]
    public async Task GetMyPendingRequestsAsync_ReturnsOnlyMembersPendingRequests()
    {
        await using LibraryDbContext context =
            TestServiceFactory.CreateContext();

        LoanRequestService service =
            TestServiceFactory.CreateLoanRequestService(context);

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

        Book b1 =
            TestServiceFactory.AddBook(
                context,
                "Mine Pending");

        Book b2 =
            TestServiceFactory.AddBook(
                context,
                "Mine Approved");

        Book b3 =
            TestServiceFactory.AddBook(
                context,
                "Other Pending");

        TestServiceFactory.AddLoanRequest(
            context,
            b1,
            member1,
            LoanRequestStatus.Pending);

        TestServiceFactory.AddLoanRequest(
            context,
            b2,
            member1,
            LoanRequestStatus.Approved);

        TestServiceFactory.AddLoanRequest(
            context,
            b3,
            member2,
            LoanRequestStatus.Pending);

        List<LoanRequestResponse> result =
            await service.GetMyPendingRequestsAsync(user1.Id);

        Assert.Single(result);

        Assert.Equal(
            "Mine Pending",
            result[0].BookTitle);

        Assert.Equal(
            LoanRequestStatus.Pending,
            result[0].Status);
    }


    [Fact]
    public async Task GetPendingRequestsAsync_AppliesSearchSortAndPagination()
    {
        await using LibraryDbContext context =
            TestServiceFactory.CreateContext();

        LoanRequestService service =
            TestServiceFactory.CreateLoanRequestService(context);

        (_, Member member) =
            await TestServiceFactory.AddMemberAsync(
                context,
                fullName: "Cem Member");

        Book b1 =
            TestServiceFactory.AddBook(
                context,
                "Alpha");

        Book b2 =
            TestServiceFactory.AddBook(
                context,
                "Beta");

        Book b3 =
            TestServiceFactory.AddBook(
                context,
                "Gamma");

        TestServiceFactory.AddLoanRequest(
            context,
            b1,
            member,
            LoanRequestStatus.Pending,
            requestDate: DateTime.UtcNow.AddMinutes(-3));

        TestServiceFactory.AddLoanRequest(
            context,
            b2,
            member,
            LoanRequestStatus.Pending,
            requestDate: DateTime.UtcNow.AddMinutes(-2));

        TestServiceFactory.AddLoanRequest(
            context,
            b3,
            member,
            LoanRequestStatus.Approved,
            requestDate: DateTime.UtcNow.AddMinutes(-1));

        PagedResponse<LoanRequestResponse> result =
            await service.GetPendingRequestsAsync(
                new LoanRequestQuery
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