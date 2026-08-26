using LibraryApi.DTOs.Auth;

namespace LibraryApi.Services.Interfaces;

public interface IAuthService
{
    Task RegisterAsync(RegisterRequest request);

    Task<AuthResponse> LoginAsync(LoginRequest request);

    Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request);

    Task LogoutAsync(int UserId);

    Task<CurrentUserResponse> GetCurrentUserAsync(int userId);
}