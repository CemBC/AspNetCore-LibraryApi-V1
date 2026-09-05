using LibraryApi.DTOs.Auth;

namespace LibraryApi.Services.Interfaces;

public interface IAuthService
{
    Task RegisterAsync(RegisterRequest request);

    Task<AuthResponse> LoginAsync(LoginRequest request);

    Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request);

    Task LogoutAsync(int UserId);

    Task<CurrentUserResponse> GetCurrentUserAsync(int userId);

    Task<CurrentUserResponse> UpdateProfileAsync(int userId, UpdateProfileRequest request);

    Task VerifyEmailAsync(VerifyEmailRequest request);

    Task ResendVerificationCodeAsync(string email);

    Task SendForgotPasswordCodeAsync(string email);

    Task ResetPasswordAsync(ResetPasswordRequest request);

    Task SendChangePasswordCodeAsync(int userId,SendChangePasswordCodeRequest request);

    Task ChangePasswordAsync(int userId,ChangePasswordRequest request);

    Task SendChangeEmailCodeAsync(int userId, SendChangeEmailCodeRequest request);

    Task<CurrentUserResponse> ChangeEmailAsync(int userId, ChangeEmailRequest request);
}