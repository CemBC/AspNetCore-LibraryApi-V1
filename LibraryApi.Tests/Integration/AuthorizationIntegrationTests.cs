using LibraryApi.DTOs.Auth;
using LibraryApi.DTOs.Books;
using LibraryApi.DTOs.LoanRequests;
using LibraryApi.Models;
using System.Net;
using System.Net.Http.Json;

namespace LibraryApi.Tests.Integration;

public class AuthorizationIntegrationTests
{
    [Fact]
    public async Task Books_GetAll_MemberCanAccess()
    {
        using CustomWebApplicationFactory factory = new();
        await factory.ResetDatabaseAsync();
        await factory.SeedMemberAsync(email: "member@test.com");

        HttpClient client = factory.CreateClient();
        AuthResponse login = await client.LoginAsync("member@test.com");
        client.UseBearerToken(login.AccessToken);

        HttpResponseMessage response = await client.GetAsync("/api/Books");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Books_Create_MemberGets403_AdminGets201()
    {
        using CustomWebApplicationFactory factory = new();
        await factory.ResetDatabaseAsync();
        await factory.SeedMemberAsync(email: "member@test.com");
        await factory.SeedAdminAsync(email: "admin@test.com");

        HttpClient client = factory.CreateClient();

        AuthResponse memberLogin = await client.LoginAsync("member@test.com");
        client.UseBearerToken(memberLogin.AccessToken);

        HttpResponseMessage memberResponse = await client.PostAsJsonAsync(
            "/api/Books",
            new CreateBookRequest
            {
                Title = "Member Cannot Create",
                Author = "Author"
            });

        Assert.Equal(HttpStatusCode.Forbidden, memberResponse.StatusCode);

        AuthResponse adminLogin = await client.LoginAsync("admin@test.com");
        client.UseBearerToken(adminLogin.AccessToken);

        HttpResponseMessage adminResponse = await client.PostAsJsonAsync(
            "/api/Books",
            new CreateBookRequest
            {
                Title = "Admin Can Create",
                Author = "Author"
            });

        Assert.Equal(HttpStatusCode.Created, adminResponse.StatusCode);
    }

    [Fact]
    public async Task MembersController_MemberGets403_AdminGets200()
    {
        using CustomWebApplicationFactory factory = new();
        await factory.ResetDatabaseAsync();
        await factory.SeedMemberAsync(email: "member@test.com");
        await factory.SeedAdminAsync(email: "admin@test.com");

        HttpClient client = factory.CreateClient();

        AuthResponse memberLogin = await client.LoginAsync("member@test.com");
        client.UseBearerToken(memberLogin.AccessToken);

        HttpResponseMessage memberResponse = await client.GetAsync("/api/Members");
        Assert.Equal(HttpStatusCode.Forbidden, memberResponse.StatusCode);

        AuthResponse adminLogin = await client.LoginAsync("admin@test.com");
        client.UseBearerToken(adminLogin.AccessToken);

        HttpResponseMessage adminResponse = await client.GetAsync("/api/Members");
        Assert.Equal(HttpStatusCode.OK, adminResponse.StatusCode);
    }

    [Fact]
    public async Task Loans_GetAll_MemberGets403_AdminGets200()
    {
        using CustomWebApplicationFactory factory = new();
        await factory.ResetDatabaseAsync();
        await factory.SeedMemberAsync(email: "member@test.com");
        await factory.SeedAdminAsync(email: "admin@test.com");

        HttpClient client = factory.CreateClient();

        AuthResponse memberLogin = await client.LoginAsync("member@test.com");
        client.UseBearerToken(memberLogin.AccessToken);

        HttpResponseMessage memberResponse = await client.GetAsync("/api/Loans");
        Assert.Equal(HttpStatusCode.Forbidden, memberResponse.StatusCode);

        AuthResponse adminLogin = await client.LoginAsync("admin@test.com");
        client.UseBearerToken(adminLogin.AccessToken);

        HttpResponseMessage adminResponse = await client.GetAsync("/api/Loans");
        Assert.Equal(HttpStatusCode.OK, adminResponse.StatusCode);
    }

    [Fact]
    public async Task Loans_My_MemberGets200_AdminGets403()
    {
        using CustomWebApplicationFactory factory = new();
        await factory.ResetDatabaseAsync();
        await factory.SeedMemberAsync(email: "member@test.com");
        await factory.SeedAdminAsync(email: "admin@test.com");

        HttpClient client = factory.CreateClient();

        AuthResponse memberLogin = await client.LoginAsync("member@test.com");
        client.UseBearerToken(memberLogin.AccessToken);

        HttpResponseMessage memberResponse = await client.GetAsync("/api/Loans/my");
        Assert.Equal(HttpStatusCode.OK, memberResponse.StatusCode);

        AuthResponse adminLogin = await client.LoginAsync("admin@test.com");
        client.UseBearerToken(adminLogin.AccessToken);

        HttpResponseMessage adminResponse = await client.GetAsync("/api/Loans/my");
        Assert.Equal(HttpStatusCode.Forbidden, adminResponse.StatusCode);
    }

    [Fact]
    public async Task LoanRequests_Create_MemberGets200_AdminGets403()
    {
        using CustomWebApplicationFactory factory = new();
        await factory.ResetDatabaseAsync();
        await factory.SeedMemberAsync(email: "member@test.com");
        await factory.SeedAdminAsync(email: "admin@test.com");
        Book book = await factory.SeedBookAsync();

        HttpClient client = factory.CreateClient();

        AuthResponse memberLogin = await client.LoginAsync("member@test.com");
        client.UseBearerToken(memberLogin.AccessToken);

        HttpResponseMessage memberResponse = await client.PostAsJsonAsync(
            "/api/LoanRequests",
            new CreateLoanRequestDto { BookId = book.Id });

        Assert.Equal(HttpStatusCode.OK, memberResponse.StatusCode);

        Book secondBook = await factory.SeedBookAsync("Second Book", "Author");

        AuthResponse adminLogin = await client.LoginAsync("admin@test.com");
        client.UseBearerToken(adminLogin.AccessToken);

        HttpResponseMessage adminResponse = await client.PostAsJsonAsync(
            "/api/LoanRequests",
            new CreateLoanRequestDto { BookId = secondBook.Id });

        Assert.Equal(HttpStatusCode.Forbidden, adminResponse.StatusCode);
    }

    [Fact]
    public async Task LoanRequests_Pending_AdminGets200_MemberGets403()
    {
        using CustomWebApplicationFactory factory = new();
        await factory.ResetDatabaseAsync();
        await factory.SeedMemberAsync(email: "member@test.com");
        await factory.SeedAdminAsync(email: "admin@test.com");

        HttpClient client = factory.CreateClient();

        AuthResponse memberLogin = await client.LoginAsync("member@test.com");
        client.UseBearerToken(memberLogin.AccessToken);

        HttpResponseMessage memberResponse = await client.GetAsync("/api/LoanRequests/pending");
        Assert.Equal(HttpStatusCode.Forbidden, memberResponse.StatusCode);

        AuthResponse adminLogin = await client.LoginAsync("admin@test.com");
        client.UseBearerToken(adminLogin.AccessToken);

        HttpResponseMessage adminResponse = await client.GetAsync("/api/LoanRequests/pending");
        Assert.Equal(HttpStatusCode.OK, adminResponse.StatusCode);
    }

    [Fact]
    public async Task LoanExtensions_Pending_AdminGets200_MemberGets403()
    {
        using CustomWebApplicationFactory factory = new();
        await factory.ResetDatabaseAsync();
        await factory.SeedMemberAsync(email: "member@test.com");
        await factory.SeedAdminAsync(email: "admin@test.com");

        HttpClient client = factory.CreateClient();

        AuthResponse memberLogin = await client.LoginAsync("member@test.com");
        client.UseBearerToken(memberLogin.AccessToken);

        HttpResponseMessage memberResponse = await client.GetAsync("/api/LoanExtensions/pending");
        Assert.Equal(HttpStatusCode.Forbidden, memberResponse.StatusCode);

        AuthResponse adminLogin = await client.LoginAsync("admin@test.com");
        client.UseBearerToken(adminLogin.AccessToken);

        HttpResponseMessage adminResponse = await client.GetAsync("/api/LoanExtensions/pending");
        Assert.Equal(HttpStatusCode.OK, adminResponse.StatusCode);
    }

    [Fact]
    public async Task AdminStatistics_MemberGets403_AdminGets200()
    {
        using CustomWebApplicationFactory factory = new();
        await factory.ResetDatabaseAsync();
        await factory.SeedMemberAsync(email: "member@test.com");
        await factory.SeedAdminAsync(email: "admin@test.com");

        HttpClient client = factory.CreateClient();

        AuthResponse memberLogin = await client.LoginAsync("member@test.com");
        client.UseBearerToken(memberLogin.AccessToken);

        HttpResponseMessage memberResponse = await client.GetAsync("/api/Admin/statistics");
        Assert.Equal(HttpStatusCode.Forbidden, memberResponse.StatusCode);

        AuthResponse adminLogin = await client.LoginAsync("admin@test.com");
        client.UseBearerToken(adminLogin.AccessToken);

        HttpResponseMessage adminResponse = await client.GetAsync("/api/Admin/statistics");
        Assert.Equal(HttpStatusCode.OK, adminResponse.StatusCode);
    }
}
