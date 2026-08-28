using LibraryApi.Data;
using LibraryApi.DTOs.Admin;
using LibraryApi.DTOs.Auth;
using LibraryApi.DTOs.LoanExtensions;
using LibraryApi.DTOs.LoanRequests;
using LibraryApi.Helpers;
using LibraryApi.Models;
using LibraryApi.Models.Status;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;

namespace LibraryApi.Tests.Integration;

public class WorkflowIntegrationTests
{
    [Fact]
    public async Task LoanRequest_ApproveFlow_CreatesLoanAndMarksBookLoaned()
    {
        using CustomWebApplicationFactory factory = new();

        await factory.ResetDatabaseAsync();

        (_, Member member) =
            await factory.SeedMemberAsync("member@test.com");

        await factory.SeedAdminAsync("admin@test.com");

        Book book =
            await factory.SeedBookAsync("Requested Book");

        HttpClient client = factory.CreateClient();

        AuthResponse memberLogin =
            await client.LoginAsync("member@test.com");

        client.UseBearerToken(memberLogin.AccessToken);

        HttpResponseMessage createResponse =
            await client.PostAsJsonAsync(
                "/api/LoanRequests",
                new CreateLoanRequestDto
                {
                    BookId = book.Id
                });

        Assert.Equal(
            HttpStatusCode.OK,
            createResponse.StatusCode);

        LoanRequestResponse created =
            (await createResponse.Content
                .ReadFromJsonAsync<LoanRequestResponse>())!;

        AuthResponse adminLogin =
            await client.LoginAsync("admin@test.com");

        client.UseBearerToken(adminLogin.AccessToken);

        HttpResponseMessage approveResponse =
            await client.PostAsync(
                $"/api/LoanRequests/{created.Id}/approve",
                null);

        Assert.Equal(
            HttpStatusCode.OK,
            approveResponse.StatusCode);

        using var scope =
            factory.Services.CreateScope();

        LibraryDbContext context =
            scope.ServiceProvider
                .GetRequiredService<LibraryDbContext>();

        LoanRequest? request =
            await context.LoanRequest.FindAsync(created.Id);

        Loan? loan =
            await context.Loans.FirstOrDefaultAsync(l =>
                l.BookId == book.Id &&
                l.MemberId == member.Id);

        Book? dbBook =
            await context.Books.FindAsync(book.Id);

        Assert.NotNull(request);
        Assert.NotNull(loan);
        Assert.NotNull(dbBook);

        Assert.Equal(
            LoanRequestStatus.Approved,
            request.Status);

        Assert.Equal(
            LoanStatus.Active,
            loan.Status);

        Assert.Equal(
            BookStatus.Loaned,
            dbBook.Status);
    }


    [Fact]
    public async Task LoanRequest_RejectFlow_MarksBookAvailableAgain()
    {
        using CustomWebApplicationFactory factory = new();

        await factory.ResetDatabaseAsync();

        (_, Member member) =
            await factory.SeedMemberAsync("member@test.com");

        await factory.SeedAdminAsync("admin@test.com");

        Book book =
            await factory.SeedBookAsync(
                "Requested Book",
                status: BookStatus.Requested);

        LoanRequest request =
            await factory.SeedLoanRequestAsync(
                book.Id,
                member.Id);

        HttpClient client =
            factory.CreateClient();

        AuthResponse adminLogin =
            await client.LoginAsync("admin@test.com");

        client.UseBearerToken(adminLogin.AccessToken);

        HttpResponseMessage response =
            await client.PostAsync(
                $"/api/LoanRequests/{request.Id}/reject",
                null);

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        using var scope =
            factory.Services.CreateScope();

        LibraryDbContext context =
            scope.ServiceProvider
                .GetRequiredService<LibraryDbContext>();

        LoanRequest? dbRequest =
            await context.LoanRequest.FindAsync(request.Id);

        Book? dbBook =
            await context.Books.FindAsync(book.Id);

        Assert.NotNull(dbRequest);
        Assert.NotNull(dbBook);

        Assert.Equal(
            LoanRequestStatus.Rejected,
            dbRequest.Status);

        Assert.Equal(
            BookStatus.Available,
            dbBook.Status);
    }


    [Fact]
    public async Task LoanExtension_ApproveFlow_AddsSevenDays()
    {
        using CustomWebApplicationFactory factory = new();

        await factory.ResetDatabaseAsync();

        (_, Member member) =
            await factory.SeedMemberAsync("member@test.com");

        await factory.SeedAdminAsync("admin@test.com");

        Book book =
            await factory.SeedBookAsync(
                "Loaned Book",
                status: BookStatus.Loaned);

        DateTime dueDate =
            DateTime.UtcNow.AddDays(3);

        Loan loan =
            await factory.SeedLoanAsync(
                book.Id,
                member.Id,
                LoanStatus.Active,
                dueDate);

        HttpClient client =
            factory.CreateClient();

        AuthResponse memberLogin =
            await client.LoginAsync("member@test.com");

        client.UseBearerToken(memberLogin.AccessToken);

        HttpResponseMessage createResponse =
            await client.PostAsJsonAsync(
                "/api/LoanExtensions",
                new CreateLoanExtensionRequestDto
                {
                    LoanId = loan.Id
                });

        Assert.Equal(
            HttpStatusCode.OK,
            createResponse.StatusCode);

        LoanExtensionResponse created =
            (await createResponse.Content
                .ReadFromJsonAsync<LoanExtensionResponse>())!;

        AuthResponse adminLogin =
            await client.LoginAsync("admin@test.com");

        client.UseBearerToken(adminLogin.AccessToken);

        HttpResponseMessage approveResponse =
            await client.PostAsync(
                $"/api/LoanExtensions/{created.Id}/approve",
                null);

        Assert.Equal(
            HttpStatusCode.OK,
            approveResponse.StatusCode);

        using var scope =
            factory.Services.CreateScope();

        LibraryDbContext context =
            scope.ServiceProvider
                .GetRequiredService<LibraryDbContext>();

        Loan? dbLoan =
            await context.Loans.FindAsync(loan.Id);

        LoanExtensionRequest? request =
            await context.LoanExtensionRequest
                .FindAsync(created.Id);

        Assert.NotNull(dbLoan);
        Assert.NotNull(request);

        Assert.Equal(
            LoanExtensionStatus.Approved,
            request.Status);

        Assert.Equal(
            dueDate.AddDays(7),
            dbLoan.DueDate);
    }


    [Fact]
    public async Task ReturnLoan_MarksLoanReturnedAndBookAvailable()
    {
        using CustomWebApplicationFactory factory = new();

        await factory.ResetDatabaseAsync();

        (_, Member member) =
            await factory.SeedMemberAsync("member@test.com");

        await factory.SeedAdminAsync("admin@test.com");

        Book book =
            await factory.SeedBookAsync(
                "Loaned Book",
                status: BookStatus.Loaned);

        Loan loan =
            await factory.SeedLoanAsync(
                book.Id,
                member.Id);

        HttpClient client =
            factory.CreateClient();

        AuthResponse adminLogin =
            await client.LoginAsync("admin@test.com");

        client.UseBearerToken(adminLogin.AccessToken);

        HttpResponseMessage response =
            await client.PutAsync(
                $"/api/Loans/{loan.Id}/return",
                null);

        Assert.Equal(
            HttpStatusCode.NoContent,
            response.StatusCode);

        using var scope =
            factory.Services.CreateScope();

        LibraryDbContext context =
            scope.ServiceProvider
                .GetRequiredService<LibraryDbContext>();

        Loan? dbLoan =
            await context.Loans.FindAsync(loan.Id);

        Book? dbBook =
            await context.Books.FindAsync(book.Id);

        Assert.NotNull(dbLoan);
        Assert.NotNull(dbBook);

        Assert.Equal(
            LoanStatus.Returned,
            dbLoan.Status);

        Assert.Equal(
            BookStatus.Available,
            dbBook.Status);
    }


    [Fact]
    public async Task CreateLoanRequest_WhenMemberHasOverdueLoan_Returns400()
    {
        using CustomWebApplicationFactory factory = new();

        await factory.ResetDatabaseAsync();

        (_, Member member) =
            await factory.SeedMemberAsync("member@test.com");

        Book overdueBook =
            await factory.SeedBookAsync(
                "Overdue",
                status: BookStatus.Loaned);

        Book newBook =
            await factory.SeedBookAsync("New Book");

        await factory.SeedLoanAsync(
            overdueBook.Id,
            member.Id,
            LoanStatus.Active,
            DateTime.UtcNow.AddDays(-1));

        HttpClient client =
            factory.CreateClient();

        AuthResponse login =
            await client.LoginAsync("member@test.com");

        client.UseBearerToken(login.AccessToken);

        HttpResponseMessage response =
            await client.PostAsJsonAsync(
                "/api/LoanRequests",
                new CreateLoanRequestDto
                {
                    BookId = newBook.Id
                });

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }


    [Fact]
    public async Task CreateLoanRequest_WhenMemberHasThreeUnreturnedLoans_Returns400()
    {
        using CustomWebApplicationFactory factory = new();

        await factory.ResetDatabaseAsync();

        (_, Member member) =
            await factory.SeedMemberAsync("member@test.com");

        for (int i = 0;
             i < LibraryRules.MaxActiveLoansPerMember;
             i++)
        {
            Book borrowed =
                await factory.SeedBookAsync(
                    $"Borrowed {i}",
                    status: BookStatus.Loaned);

            await factory.SeedLoanAsync(
                borrowed.Id,
                member.Id,
                LoanStatus.Active,
                DateTime.UtcNow.AddDays(i + 2));
        }

        Book fourth =
            await factory.SeedBookAsync("Fourth");

        HttpClient client =
            factory.CreateClient();

        AuthResponse login =
            await client.LoginAsync("member@test.com");

        client.UseBearerToken(login.AccessToken);

        HttpResponseMessage response =
            await client.PostAsJsonAsync(
                "/api/LoanRequests",
                new CreateLoanRequestDto
                {
                    BookId = fourth.Id
                });

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }


    [Fact]
    public async Task CreateExtension_WhenLoanAlreadyHasTwoApprovedExtensions_Returns400()
    {
        using CustomWebApplicationFactory factory = new();

        await factory.ResetDatabaseAsync();

        (_, Member member) =
            await factory.SeedMemberAsync("member@test.com");

        Book book =
            await factory.SeedBookAsync(
                "Loaned",
                status: BookStatus.Loaned);

        Loan loan =
            await factory.SeedLoanAsync(
                book.Id,
                member.Id,
                LoanStatus.Active,
                DateTime.UtcNow.AddDays(3));

        await factory.SeedExtensionRequestAsync(
            loan.Id,
            member.Id,
            LoanExtensionStatus.Approved);

        await factory.SeedExtensionRequestAsync(
            loan.Id,
            member.Id,
            LoanExtensionStatus.Approved);

        HttpClient client =
            factory.CreateClient();

        AuthResponse login =
            await client.LoginAsync("member@test.com");

        client.UseBearerToken(login.AccessToken);

        HttpResponseMessage response =
            await client.PostAsJsonAsync(
                "/api/LoanExtensions",
                new CreateLoanExtensionRequestDto
                {
                    LoanId = loan.Id
                });

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }


    [Fact]
    public async Task CreateExtension_WhenDueDateHasPassed_Returns400()
    {
        using CustomWebApplicationFactory factory = new();

        await factory.ResetDatabaseAsync();

        (_, Member member) =
            await factory.SeedMemberAsync("member@test.com");

        Book book =
            await factory.SeedBookAsync(
                "Loaned",
                status: BookStatus.Loaned);

        Loan loan =
            await factory.SeedLoanAsync(
                book.Id,
                member.Id,
                LoanStatus.Active,
                DateTime.UtcNow.AddMinutes(-1));

        HttpClient client =
            factory.CreateClient();

        AuthResponse login =
            await client.LoginAsync("member@test.com");

        client.UseBearerToken(login.AccessToken);

        HttpResponseMessage response =
            await client.PostAsJsonAsync(
                "/api/LoanExtensions",
                new CreateLoanExtensionRequestDto
                {
                    LoanId = loan.Id
                });

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }


    [Fact]
    public async Task AdminStatistics_ReturnsCurrentDashboardCounts()
    {
        using CustomWebApplicationFactory factory = new();

        await factory.ResetDatabaseAsync();

        (_, Member member) =
            await factory.SeedMemberAsync("member@test.com");

        await factory.SeedAdminAsync("admin@test.com");

        await factory.SeedBookAsync("Available");

        Book activeBook =
            await factory.SeedBookAsync(
                "Active",
                status: BookStatus.Loaned);

        Book requestedBook =
            await factory.SeedBookAsync(
                "Requested",
                status: BookStatus.Requested);

        Loan active =
            await factory.SeedLoanAsync(
                activeBook.Id,
                member.Id,
                LoanStatus.Active);

        await factory.SeedLoanRequestAsync(
            requestedBook.Id,
            member.Id,
            LoanRequestStatus.Pending);

        await factory.SeedExtensionRequestAsync(
            active.Id,
            member.Id,
            LoanExtensionStatus.Pending);

        HttpClient client =
            factory.CreateClient();

        AuthResponse login =
            await client.LoginAsync("admin@test.com");

        client.UseBearerToken(login.AccessToken);

        AdminStatisticsResponse? response =
            await client.GetFromJsonAsync<AdminStatisticsResponse>(
                "/api/Admin/statistics");

        Assert.NotNull(response);

        Assert.Equal(
            3,
            response.TotalBooks);

        Assert.Equal(
            1,
            response.AvailableBooks);

        Assert.Equal(
            1,
            response.TotalMembers);

        Assert.Equal(
            1,
            response.ActiveLoans);

        Assert.Equal(
            1,
            response.PendingLoanRequests);

        Assert.Equal(
            1,
            response.PendingExtensionRequests);
    }
}