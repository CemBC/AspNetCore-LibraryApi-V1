using LibraryApi.Data;
using LibraryApi.DTOs.Auth;
using LibraryApi.Exceptions;
using LibraryApi.Helpers;
using LibraryApi.Models;
using LibraryApi.Services;
using LibraryApi.Tests.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Tests.Unit;

public class AuthServiceTests
{
    [Fact]
    public async Task RegisterAsync_CreatesUserAndLinkedMember()
    {
        await using LibraryDbContext context = TestServiceFactory.CreateContext();
        AuthService service = TestServiceFactory.CreateAuthService(context);

        RegisterRequest request = new()
        {
            Email = "new@test.com",
            Password = "Secret123!",
            FullName = "New Member"
        };

        await service.RegisterAsync(request);

        User? user = await context.Users
            .Include(u => u.Member)
            .SingleOrDefaultAsync(u => u.Email == request.Email);

        Assert.NotNull(user);
        Assert.Equal("Member", user.Role);
        Assert.NotEqual(request.Password, user.PasswordHash);
        Assert.NotNull(user.Member);
        Assert.Equal(request.FullName, user.Member!.FullName);
    }

    [Fact]
    public async Task RegisterAsync_WhenEmailExists_ThrowsBadRequestException()
    {
        await using LibraryDbContext context =
            TestServiceFactory.CreateContext();

        AuthService service =
            TestServiceFactory.CreateAuthService(context);

        await TestServiceFactory.AddMemberAsync(
            context,
            email: "duplicate@test.com",
            isEmailVerified: true);

        RegisterRequest request = new()
        {
            Email = "duplicate@test.com",
            Password = "Secret123!",
            FullName = "Duplicate User"
        };

        await Assert.ThrowsAsync<BadRequestException>(() =>
            service.RegisterAsync(request));
    }

    [Fact]
    public async Task LoginAsync_WithCorrectCredentials_ReturnsTokensAndStoresHashedRefreshToken()
    {
        await using LibraryDbContext context = TestServiceFactory.CreateContext();
        AuthService service = TestServiceFactory.CreateAuthService(context);

        (User user, _) = await TestServiceFactory.AddMemberAsync(
            context,
            email: "login@test.com",
            password: "Secret123!");

        AuthResponse response = await service.LoginAsync(new LoginRequest
        {
            Email = "login@test.com",
            Password = "Secret123!"
        });

        User dbUser = await context.Users.SingleAsync(u => u.Id == user.Id);

        Assert.False(string.IsNullOrWhiteSpace(response.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(response.RefreshToken));
        Assert.Equal(TokenHasher.Hash(response.RefreshToken), dbUser.RefreshTokenHash);
        Assert.True(dbUser.RefreshTokenExpiryTime > DateTime.UtcNow);
    }

    [Fact]
    public async Task LoginAsync_WhenUserDoesNotExist_ThrowsUnauthorizedException()
    {
        await using LibraryDbContext context = TestServiceFactory.CreateContext();
        AuthService service = TestServiceFactory.CreateAuthService(context);

        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            service.LoginAsync(new LoginRequest
            {
                Email = "missing@test.com",
                Password = "Secret123!"
            }));
    }

    [Fact]
    public async Task LoginAsync_WhenPasswordIsWrong_ThrowsUnauthorizedException()
    {
        await using LibraryDbContext context = TestServiceFactory.CreateContext();
        AuthService service = TestServiceFactory.CreateAuthService(context);

        await TestServiceFactory.AddMemberAsync(
            context,
            email: "wrongpassword@test.com",
            password: "Secret123!");

        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            service.LoginAsync(new LoginRequest
            {
                Email = "wrongpassword@test.com",
                Password = "WrongPassword!"
            }));
    }

    [Fact]
    public async Task RefreshTokenAsync_WithValidToken_RotatesRefreshToken()
    {
        await using LibraryDbContext context = TestServiceFactory.CreateContext();
        AuthService service = TestServiceFactory.CreateAuthService(context);

        await TestServiceFactory.AddMemberAsync(
            context,
            email: "refresh@test.com",
            password: "Secret123!");

        AuthResponse login = await service.LoginAsync(new LoginRequest
        {
            Email = "refresh@test.com",
            Password = "Secret123!"
        });

        string oldRefreshToken = login.RefreshToken;

        AuthResponse refreshed = await service.RefreshTokenAsync(new RefreshTokenRequest
        {
            RefreshToken = oldRefreshToken
        });

        User user = await context.Users.SingleAsync(u => u.Email == "refresh@test.com");

        Assert.False(string.IsNullOrWhiteSpace(refreshed.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(refreshed.RefreshToken));
        Assert.NotEqual(oldRefreshToken, refreshed.RefreshToken);
        Assert.Equal(TokenHasher.Hash(refreshed.RefreshToken), user.RefreshTokenHash);
        Assert.NotEqual(TokenHasher.Hash(oldRefreshToken), user.RefreshTokenHash);
    }

    [Fact]
    public async Task RefreshTokenAsync_WhenTokenIsInvalid_ThrowsUnauthorizedException()
    {
        await using LibraryDbContext context = TestServiceFactory.CreateContext();
        AuthService service = TestServiceFactory.CreateAuthService(context);

        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            service.RefreshTokenAsync(new RefreshTokenRequest
            {
                RefreshToken = "invalid-refresh-token"
            }));
    }

    [Fact]
    public async Task RefreshTokenAsync_WhenTokenIsExpired_ThrowsUnauthorizedException()
    {
        await using LibraryDbContext context = TestServiceFactory.CreateContext();
        AuthService service = TestServiceFactory.CreateAuthService(context);

        (User user, _) = await TestServiceFactory.AddMemberAsync(context);

        const string refreshToken = "expired-token";

        user.RefreshTokenHash = TokenHasher.Hash(refreshToken);
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddMinutes(-1);
        await context.SaveChangesAsync();

        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            service.RefreshTokenAsync(new RefreshTokenRequest
            {
                RefreshToken = refreshToken
            }));
    }

    [Fact]
    public async Task LogoutAsync_ClearsRefreshTokenData()
    {
        await using LibraryDbContext context = TestServiceFactory.CreateContext();
        AuthService service = TestServiceFactory.CreateAuthService(context);

        (User user, _) = await TestServiceFactory.AddMemberAsync(context);

        user.RefreshTokenHash = TokenHasher.Hash("refresh");
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        await context.SaveChangesAsync();

        await service.LogoutAsync(user.Id);

        User dbUser = await context.Users.SingleAsync(u => u.Id == user.Id);

        Assert.Null(dbUser.RefreshTokenHash);
        Assert.Null(dbUser.RefreshTokenExpiryTime);
    }

    [Fact]
    public async Task GetCurrentUserAsync_ReturnsMemberInformation()
    {
        await using LibraryDbContext context = TestServiceFactory.CreateContext();
        AuthService service = TestServiceFactory.CreateAuthService(context);

        (User user, Member member) = await TestServiceFactory.AddMemberAsync(
            context,
            email: "me@test.com",
            fullName: "Current User");

        CurrentUserResponse response = await service.GetCurrentUserAsync(user.Id);

        Assert.Equal(user.Id, response.Id);
        Assert.Equal("me@test.com", response.Email);
        Assert.Equal("Member", response.Role);
        Assert.Equal(member.Id, response.MemberId);
        Assert.Equal("Current User", response.FullName);
    }
}
