using LibraryApi.Data;
using LibraryApi.DTOs.Auth;
using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;

namespace LibraryApi.Tests.Integration;

public class AuthIntegrationTests
{
    [Fact]
    public async Task Register_ValidRequest_Returns201AndCreatesUserWithMember()
    {
        using CustomWebApplicationFactory factory = new();
        await factory.ResetDatabaseAsync();

        HttpClient client = factory.CreateClient();

        HttpResponseMessage response = await client.PostAsJsonAsync(
            "/api/Auth/register",
            new RegisterRequest
            {
                Email = "register@test.com",
                Password = "Secret123!",
                FullName = "Register Member"
            });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        using var scope = factory.Services.CreateScope();
        LibraryDbContext context = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();

        User? user = await context.Users
            .Include(u => u.Member)
            .SingleOrDefaultAsync(u => u.Email == "register@test.com");

        Assert.NotNull(user);
        Assert.NotNull(user.Member);
        Assert.Equal("Register Member", user.Member!.FullName);
    }

    [Fact]
    public async Task Register_InvalidBody_Returns400()
    {
        using CustomWebApplicationFactory factory = new();
        await factory.ResetDatabaseAsync();

        HttpClient client = factory.CreateClient();

        HttpResponseMessage response = await client.PostAsJsonAsync(
            "/api/Auth/register",
            new RegisterRequest
            {
                Email = "bad-email",
                Password = "123",
                FullName = "1234"
            });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsAccessAndRefreshTokens()
    {
        using CustomWebApplicationFactory factory = new();
        await factory.ResetDatabaseAsync();
        await factory.SeedMemberAsync(email: "login@test.com");

        HttpClient client = factory.CreateClient();

        HttpResponseMessage response = await client.PostAsJsonAsync(
            "/api/Auth/login",
            new LoginRequest
            {
                Email = "login@test.com",
                Password = "Secret123!"
            });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        AuthResponse? body = await response.Content.ReadFromJsonAsync<AuthResponse>();

        Assert.NotNull(body);
        Assert.False(string.IsNullOrWhiteSpace(body.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(body.RefreshToken));
    }

    [Fact]
    public async Task Login_WrongPassword_Returns401()
    {
        using CustomWebApplicationFactory factory = new();
        await factory.ResetDatabaseAsync();
        await factory.SeedMemberAsync(email: "login@test.com");

        HttpClient client = factory.CreateClient();

        HttpResponseMessage response = await client.PostAsJsonAsync(
            "/api/Auth/login",
            new LoginRequest
            {
                Email = "login@test.com",
                Password = "WrongPassword!"
            });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Me_WithoutToken_Returns401()
    {
        using CustomWebApplicationFactory factory = new();
        await factory.ResetDatabaseAsync();

        HttpClient client = factory.CreateClient();

        HttpResponseMessage response = await client.GetAsync("/api/Auth/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Me_WithMemberToken_ReturnsCurrentUserAndMemberData()
    {
        using CustomWebApplicationFactory factory = new();
        await factory.ResetDatabaseAsync();
        (User user, Member member) = await factory.SeedMemberAsync(
            email: "me@test.com",
            fullName: "Me Member");

        HttpClient client = factory.CreateClient();
        AuthResponse login = await client.LoginAsync("me@test.com");
        client.UseBearerToken(login.AccessToken);

        CurrentUserResponse? response =
            await client.GetFromJsonAsync<CurrentUserResponse>("/api/Auth/me");

        Assert.NotNull(response);
        Assert.Equal(user.Id, response.Id);
        Assert.Equal(member.Id, response.MemberId);
        Assert.Equal("Me Member", response.FullName);
        Assert.Equal("Member", response.Role);
    }

    [Fact]
    public async Task Refresh_ValidToken_RotatesTokenPairAndInvalidatesOldRefreshToken()
    {
        using CustomWebApplicationFactory factory = new();
        await factory.ResetDatabaseAsync();
        await factory.SeedMemberAsync(email: "refresh@test.com");

        HttpClient client = factory.CreateClient();
        AuthResponse login = await client.LoginAsync("refresh@test.com");

        HttpResponseMessage refreshResponse = await client.PostAsJsonAsync(
            "/api/Auth/refresh",
            new RefreshTokenRequest
            {
                RefreshToken = login.RefreshToken
            });

        Assert.Equal(HttpStatusCode.OK, refreshResponse.StatusCode);

        AuthResponse refreshed =
            (await refreshResponse.Content.ReadFromJsonAsync<AuthResponse>())!;

        Assert.NotEqual(login.RefreshToken, refreshed.RefreshToken);

        HttpResponseMessage oldTokenResponse = await client.PostAsJsonAsync(
            "/api/Auth/refresh",
            new RefreshTokenRequest
            {
                RefreshToken = login.RefreshToken
            });

        Assert.Equal(HttpStatusCode.Unauthorized, oldTokenResponse.StatusCode);
    }

    [Fact]
    public async Task Logout_InvalidatesRefreshToken()
    {
        using CustomWebApplicationFactory factory = new();
        await factory.ResetDatabaseAsync();
        await factory.SeedMemberAsync(email: "logout@test.com");

        HttpClient client = factory.CreateClient();
        AuthResponse login = await client.LoginAsync("logout@test.com");
        client.UseBearerToken(login.AccessToken);

        HttpResponseMessage logout = await client.PostAsync("/api/Auth/logout", null);

        Assert.Equal(HttpStatusCode.NoContent, logout.StatusCode);

        client.DefaultRequestHeaders.Authorization = null;

        HttpResponseMessage refresh = await client.PostAsJsonAsync(
            "/api/Auth/refresh",
            new RefreshTokenRequest
            {
                RefreshToken = login.RefreshToken
            });

        Assert.Equal(HttpStatusCode.Unauthorized, refresh.StatusCode);
    }
}
