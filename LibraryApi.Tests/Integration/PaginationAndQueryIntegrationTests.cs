using LibraryApi.DTOs.Auth;
using LibraryApi.DTOs.Books;
using LibraryApi.DTOs.Common;
using LibraryApi.DTOs.LoanExtensions;
using LibraryApi.DTOs.LoanRequests;
using LibraryApi.DTOs.Loans;
using LibraryApi.DTOs.Members;
using LibraryApi.Models;
using LibraryApi.Models.Status;
using System.Net;
using System.Net.Http.Json;

namespace LibraryApi.Tests.Integration;

public class PaginationAndQueryIntegrationTests
{
    [Fact]
    public async Task Books_QuerySupportsPaginationSearchStatusAndSort()
    {
        using CustomWebApplicationFactory factory = new();
        await factory.ResetDatabaseAsync();
        await factory.SeedMemberAsync(email: "member@test.com");

        await factory.SeedBookAsync("Dune", "Frank Herbert", BookStatus.Available);
        await factory.SeedBookAsync("Dune Messiah", "Frank Herbert", BookStatus.Available);
        await factory.SeedBookAsync("1984", "George Orwell", BookStatus.Loaned);

        HttpClient client = factory.CreateClient();
        AuthResponse login = await client.LoginAsync("member@test.com");
        client.UseBearerToken(login.AccessToken);

        PagedResponse<BookResponse>? result =
            await client.GetFromJsonAsync<PagedResponse<BookResponse>>(
                "/api/Books?page=1&pageSize=1&search=Dune&status=1&sortBy=title&descending=true");

        Assert.NotNull(result);
        Assert.Equal(2, result.TotalCount);
        Assert.Equal(2, result.TotalPages);
        Assert.Single(result.Items);
        Assert.Equal("Dune Messiah", result.Items[0].Title);
    }

    [Fact]
    public async Task Books_InvalidPagination_Returns400()
    {
        using CustomWebApplicationFactory factory = new();
        await factory.ResetDatabaseAsync();
        await factory.SeedMemberAsync(email: "member@test.com");

        HttpClient client = factory.CreateClient();
        AuthResponse login = await client.LoginAsync("member@test.com");
        client.UseBearerToken(login.AccessToken);

        HttpResponseMessage response =
            await client.GetAsync("/api/Books?page=0&pageSize=101");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Members_QuerySupportsPaginationSearchAndSort()
    {
        using CustomWebApplicationFactory factory = new();
        await factory.ResetDatabaseAsync();

        await factory.SeedMemberAsync("ali@test.com", fullName: "Ali Yilmaz");
        await factory.SeedMemberAsync("ayse@test.com", fullName: "Ayse Demir");
        await factory.SeedAdminAsync("admin@test.com");

        HttpClient client = factory.CreateClient();
        AuthResponse login = await client.LoginAsync("admin@test.com");
        client.UseBearerToken(login.AccessToken);

        PagedResponse<MemberResponse>? result =
            await client.GetFromJsonAsync<PagedResponse<MemberResponse>>(
                "/api/Members?page=1&pageSize=1&search=a&sortBy=fullName");

        Assert.NotNull(result);
        Assert.True(result.TotalCount >= 2);
        Assert.Single(result.Items);
        Assert.Equal("Ali Yilmaz", result.Items[0].FullName);
    }

    [Fact]
    public async Task Loans_QuerySupportsStatusSearchSortAndPagination()
    {
        using CustomWebApplicationFactory factory = new();
        await factory.ResetDatabaseAsync();

        (_, Member member) = await factory.SeedMemberAsync(
            "member@test.com",
            fullName: "Cem Member");

        await factory.SeedAdminAsync("admin@test.com");

        Book alpha = await factory.SeedBookAsync("Alpha", status: BookStatus.Loaned);
        Book beta = await factory.SeedBookAsync("Beta", status: BookStatus.Loaned);
        Book returned = await factory.SeedBookAsync("Returned", status: BookStatus.Available);

        await factory.SeedLoanAsync(alpha.Id, member.Id, LoanStatus.Active, DateTime.UtcNow.AddDays(4));
        await factory.SeedLoanAsync(beta.Id, member.Id, LoanStatus.Active, DateTime.UtcNow.AddDays(2));
        await factory.SeedLoanAsync(returned.Id, member.Id, LoanStatus.Returned, DateTime.UtcNow.AddDays(1), DateTime.UtcNow);

        HttpClient client = factory.CreateClient();
        AuthResponse login = await client.LoginAsync("admin@test.com");
        client.UseBearerToken(login.AccessToken);

        PagedResponse<LoanResponse>? result =
            await client.GetFromJsonAsync<PagedResponse<LoanResponse>>(
                "/api/Loans?page=1&pageSize=1&search=Cem&status=1&sortBy=dueDate");

        Assert.NotNull(result);
        Assert.Equal(2, result.TotalCount);
        Assert.Equal(2, result.TotalPages);
        Assert.Single(result.Items);
        Assert.Equal("Beta", result.Items[0].BookTitle);
    }

    [Fact]
    public async Task LoanRequests_PendingSupportsSearchSortAndPagination()
    {
        using CustomWebApplicationFactory factory = new();
        await factory.ResetDatabaseAsync();

        (_, Member member) = await factory.SeedMemberAsync(
            "member@test.com",
            fullName: "Cem Member");

        await factory.SeedAdminAsync("admin@test.com");

        Book alpha = await factory.SeedBookAsync("Alpha");
        Book beta = await factory.SeedBookAsync("Beta");
        Book gamma = await factory.SeedBookAsync("Gamma");

        await factory.SeedLoanRequestAsync(alpha.Id, member.Id, LoanRequestStatus.Pending, "LR-AAA111");
        await factory.SeedLoanRequestAsync(beta.Id, member.Id, LoanRequestStatus.Pending, "LR-BBB222");
        await factory.SeedLoanRequestAsync(gamma.Id, member.Id, LoanRequestStatus.Approved, "LR-CCC333");

        HttpClient client = factory.CreateClient();
        AuthResponse login = await client.LoginAsync("admin@test.com");
        client.UseBearerToken(login.AccessToken);

        PagedResponse<LoanRequestResponse>? result =
            await client.GetFromJsonAsync<PagedResponse<LoanRequestResponse>>(
                "/api/LoanRequests/pending?page=1&pageSize=1&search=Cem&sortBy=bookTitle&descending=true");

        Assert.NotNull(result);
        Assert.Equal(2, result.TotalCount);
        Assert.Equal(2, result.TotalPages);
        Assert.Single(result.Items);
        Assert.Equal("Beta", result.Items[0].BookTitle);
    }

    [Fact]
    public async Task LoanExtensions_PendingSupportsSearchSortAndPagination()
    {
        using CustomWebApplicationFactory factory = new();
        await factory.ResetDatabaseAsync();

        (_, Member member) = await factory.SeedMemberAsync(
            "member@test.com",
            fullName: "Cem Member");

        await factory.SeedAdminAsync("admin@test.com");

        Book alpha = await factory.SeedBookAsync("Alpha", status: BookStatus.Loaned);
        Book beta = await factory.SeedBookAsync("Beta", status: BookStatus.Loaned);
        Book gamma = await factory.SeedBookAsync("Gamma", status: BookStatus.Loaned);

        Loan l1 = await factory.SeedLoanAsync(alpha.Id, member.Id);
        Loan l2 = await factory.SeedLoanAsync(beta.Id, member.Id);
        Loan l3 = await factory.SeedLoanAsync(gamma.Id, member.Id);

        await factory.SeedExtensionRequestAsync(l1.Id, member.Id, LoanExtensionStatus.Pending);
        await factory.SeedExtensionRequestAsync(l2.Id, member.Id, LoanExtensionStatus.Pending);
        await factory.SeedExtensionRequestAsync(l3.Id, member.Id, LoanExtensionStatus.Approved);

        HttpClient client = factory.CreateClient();
        AuthResponse login = await client.LoginAsync("admin@test.com");
        client.UseBearerToken(login.AccessToken);

        PagedResponse<LoanExtensionResponse>? result =
            await client.GetFromJsonAsync<PagedResponse<LoanExtensionResponse>>(
                "/api/LoanExtensions/pending?page=1&pageSize=1&search=Cem&sortBy=bookTitle&descending=true");

        Assert.NotNull(result);
        Assert.Equal(2, result.TotalCount);
        Assert.Equal(2, result.TotalPages);
        Assert.Single(result.Items);
        Assert.Equal("Beta", result.Items[0].BookTitle);
    }

    [Fact]
    public async Task ActiveAndOverdueEndpoints_ReturnCorrectPagedStatuses()
    {
        using CustomWebApplicationFactory factory = new();
        await factory.ResetDatabaseAsync();

        (_, Member member) = await factory.SeedMemberAsync("member@test.com");
        await factory.SeedAdminAsync("admin@test.com");

        Book activeBook = await factory.SeedBookAsync("Active", status: BookStatus.Loaned);
        Book overdueBook = await factory.SeedBookAsync("Overdue", status: BookStatus.Loaned);

        await factory.SeedLoanAsync(activeBook.Id, member.Id, LoanStatus.Active, DateTime.UtcNow.AddDays(3));
        await factory.SeedLoanAsync(overdueBook.Id, member.Id, LoanStatus.Overdue, DateTime.UtcNow.AddDays(-3));

        HttpClient client = factory.CreateClient();
        AuthResponse login = await client.LoginAsync("admin@test.com");
        client.UseBearerToken(login.AccessToken);

        PagedResponse<LoanResponse>? active =
            await client.GetFromJsonAsync<PagedResponse<LoanResponse>>("/api/Loans/active");

        PagedResponse<LoanResponse>? overdue =
            await client.GetFromJsonAsync<PagedResponse<LoanResponse>>("/api/Loans/overdue");

        Assert.NotNull(active);
        Assert.NotNull(overdue);
        Assert.Single(active.Items);
        Assert.Single(overdue.Items);
        Assert.Equal(LoanStatus.Active, active.Items[0].Status);
        Assert.Equal(LoanStatus.Overdue, overdue.Items[0].Status);
    }
}
