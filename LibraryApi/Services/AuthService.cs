using LibraryApi.Data;
using LibraryApi.DTOs.Auth;
using LibraryApi.Exceptions;
using LibraryApi.Helpers;
using LibraryApi.Models;
using LibraryApi.Models.Status;
using LibraryApi.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace LibraryApi.Services
{
    public class AuthService : IAuthService
    {
        private readonly LibraryDbContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;

        private readonly IConfiguration _configuration;

        private readonly ILogger<AuthService> _logger;

        private readonly IEmailService _emailService;



        public AuthService(LibraryDbContext context, IPasswordHasher<User> passwordHasher , IConfiguration configuration , ILogger<AuthService> logger , IEmailService emailService)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _configuration = configuration;
            _logger = logger;
            _emailService = emailService;
        }

        public async Task SendChangePasswordCodeAsync(int userId,SendChangePasswordCodeRequest request)
        {
            User? user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user is null)
            {
                throw new NotFoundException("User not found");
            }

            PasswordVerificationResult passwordResult =
                _passwordHasher.VerifyHashedPassword(
                    user,
                    user.PasswordHash,
                    request.CurrentPassword);

            if (passwordResult == PasswordVerificationResult.Failed)
            {
                throw new BadRequestException(
                    "Current password is incorrect");
            }

            VerificationCode? latestCode = await _context.VerificationCodes
                .Where(x =>
                    x.UserId == user.Id &&
                    x.Purpose == VerificationPurpose.ChangePassword)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync();

            if (
                latestCode is not null &&
                latestCode.CreatedAt > DateTime.UtcNow.AddMinutes(-1))
            {
                throw new BadRequestException(
                    "Please wait before requesting another verification code");
            }

            List<VerificationCode> oldCodes = await _context.VerificationCodes
                .Where(x =>
                    x.UserId == user.Id &&
                    x.Purpose == VerificationPurpose.ChangePassword &&
                    x.UsedAt == null)
                .ToListAsync();

            foreach (VerificationCode oldCode in oldCodes)
            {
                oldCode.UsedAt = DateTime.UtcNow;
            }

            string code = CreateVerificationCode();

            VerificationCode verificationCode = new VerificationCode
            {
                UserId = user.Id,
                CodeHash = TokenHasher.Hash(code),
                Purpose = VerificationPurpose.ChangePassword,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(10)
            };

            await _context.VerificationCodes.AddAsync(verificationCode);

            await _context.SaveChangesAsync();

            string html = $"""
        <h2>Library Password Change</h2>

        <p>We received a request to change your account password.</p>

        <p>Your verification code is:</p>

        <h1>{code}</h1>

        <p>This code will expire in 10 minutes.</p>

        <p>If you did not request this change, you can ignore this email.</p>
        """;

            await _emailService.SendAsync(
                user.Email,
                "Confirm your Library password change",
                html);

            _logger.LogInformation(
                "Password change verification code sent for user {UserId}.",
                user.Id);
        }

        private async Task SendEmailVerificationCodeAsync(User user)
        {
            List<VerificationCode> oldCodes =await _context.VerificationCodes.Where(x =>
                        x.UserId == user.Id &&
                        x.Purpose == VerificationPurpose.EmailVerification &&
                        x.UsedAt == null)
                    .ToListAsync();

            foreach (VerificationCode oldCode in oldCodes)
            {
                oldCode.UsedAt = DateTime.UtcNow;
            }

            string code = CreateVerificationCode();

            VerificationCode verificationCode = new VerificationCode
            {
                UserId = user.Id,
                CodeHash = TokenHasher.Hash(code),
                Purpose = VerificationPurpose.EmailVerification,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(10)
            };

            await _context.VerificationCodes.AddAsync(verificationCode);

            await _context.SaveChangesAsync();

            string html = $"""
                <h2>Library Email Verification</h2>

                <p>Your verification code is:</p>

                <h1>{code}</h1>

                <p>This code will expire in 10 minutes.</p>
             """;

            await _emailService.SendAsync(user.Email,"Verify your Library account",html);
        }

        public async Task VerifyEmailAsync(VerifyEmailRequest request)
        {
            User? user =await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user is null)
            {
                throw new BadRequestException(
                    "Invalid verification request");
            }

            if (user.IsEmailVerified)
            {
                throw new BadRequestException(
                    "Email is already verified");
            }

            string codeHash =
                TokenHasher.Hash(request.Code);

            VerificationCode? verificationCode =
                await _context.VerificationCodes
                    .Where(x =>
                        x.UserId == user.Id &&
                        x.Purpose == VerificationPurpose.EmailVerification &&
                        x.CodeHash == codeHash &&
                        x.UsedAt == null)
                    .OrderByDescending(x => x.CreatedAt)
                    .FirstOrDefaultAsync();

            if (verificationCode is null)
            {
                throw new BadRequestException(
                    "Invalid verification code");
            }

            if (verificationCode.ExpiresAt <= DateTime.UtcNow)
            {
                throw new BadRequestException(
                    "Verification code has expired");
            }

            verificationCode.UsedAt = DateTime.UtcNow;

            user.IsEmailVerified = true;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Email verified successfully for user {UserId}.", user.Id);
        }

        public async Task ResendVerificationCodeAsync( string email)
        {
            User? user = await _context.Users.FirstOrDefaultAsync( u => u.Email == email);

            if (user is null)
            {
                return;
            }

            if (user.IsEmailVerified)
            {
                return;
            }

            VerificationCode? latestCode =
                await _context.VerificationCodes
                    .Where(x =>
                        x.UserId == user.Id &&
                        x.Purpose == VerificationPurpose.EmailVerification)
                    .OrderByDescending(x => x.CreatedAt)
                    .FirstOrDefaultAsync();

            if (latestCode is not null && latestCode.CreatedAt > DateTime.UtcNow.AddMinutes(-1))
            {
                throw new BadRequestException("Please wait before requesting another verification code");
            }

            await SendEmailVerificationCodeAsync(user);
        }


        public async Task ChangePasswordAsync(int userId,ChangePasswordRequest request)
        {
            User? user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user is null)
            {
                throw new NotFoundException("User not found");
            }

            PasswordVerificationResult passwordResult =
                _passwordHasher.VerifyHashedPassword(
                    user,
                    user.PasswordHash,
                    request.CurrentPassword);

            if (passwordResult == PasswordVerificationResult.Failed)
            {
                throw new BadRequestException(
                    "Current password is incorrect");
            }

            PasswordVerificationResult newPasswordResult =
                _passwordHasher.VerifyHashedPassword(
                    user,
                    user.PasswordHash,
                    request.NewPassword);

            if (newPasswordResult != PasswordVerificationResult.Failed)
            {
                throw new BadRequestException(
                    "New password must be different from current password");
            }

            string codeHash = TokenHasher.Hash(request.Code);

            VerificationCode? verificationCode =
                await _context.VerificationCodes
                    .Where(x =>
                        x.UserId == user.Id &&
                        x.Purpose == VerificationPurpose.ChangePassword &&
                        x.CodeHash == codeHash &&
                        x.UsedAt == null)
                    .OrderByDescending(x => x.CreatedAt)
                    .FirstOrDefaultAsync();

            if (verificationCode is null)
            {
                throw new BadRequestException(
                    "Invalid verification code");
            }

            if (verificationCode.ExpiresAt <= DateTime.UtcNow)
            {
                throw new BadRequestException(
                    "Verification code has expired");
            }

            verificationCode.UsedAt = DateTime.UtcNow;

            user.PasswordHash = _passwordHasher.HashPassword(
                user,
                request.NewPassword);

            user.RefreshTokenHash = null;
            user.RefreshTokenExpiryTime = null;

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Password changed successfully for user {UserId}.",
                user.Id);
        }

        public async Task SendForgotPasswordCodeAsync(string email)
        {
            User? user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);

            if (user is null)
            {
                return;
            }

            VerificationCode? latestCode = await _context.VerificationCodes
                .Where(x =>
                    x.UserId == user.Id &&
                    x.Purpose == VerificationPurpose.ForgotPassword)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync();

            if (
                latestCode is not null &&
                latestCode.CreatedAt > DateTime.UtcNow.AddMinutes(-1))
            {
                throw new BadRequestException(
                    "Please wait before requesting another password reset code");
            }

            List<VerificationCode> oldCodes = await _context.VerificationCodes
                .Where(x =>
                    x.UserId == user.Id &&
                    x.Purpose == VerificationPurpose.ForgotPassword &&
                    x.UsedAt == null)
                .ToListAsync();

            foreach (VerificationCode oldCode in oldCodes)
            {
                oldCode.UsedAt = DateTime.UtcNow;
            }

            string code = CreateVerificationCode();

            VerificationCode verificationCode = new VerificationCode
            {
                UserId = user.Id,
                CodeHash = TokenHasher.Hash(code),
                Purpose = VerificationPurpose.ForgotPassword,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(10)
            };

            await _context.VerificationCodes.AddAsync(verificationCode);

            await _context.SaveChangesAsync();

            string html = $"""
        <h2>Library Password Reset</h2>

        <p>We received a request to reset your password.</p>

        <p>Your password reset code is:</p>

        <h1>{code}</h1>

        <p>This code will expire in 10 minutes.</p>

        <p>If you did not request a password reset, you can ignore this email.</p>
        """;

            await _emailService.SendAsync(
                user.Email,
                "Reset your Library password",
                html);

            _logger.LogInformation(
                "Password reset code sent for user {UserId}.",
                user.Id);
        }

        public async Task ResetPasswordAsync(ResetPasswordRequest request)
        {
            User? user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user is null)
            {
                throw new BadRequestException(
                    "Invalid password reset request");
            }

            string codeHash = TokenHasher.Hash(request.Code);

            VerificationCode? verificationCode =
                await _context.VerificationCodes
                    .Where(x =>
                        x.UserId == user.Id &&
                        x.Purpose == VerificationPurpose.ForgotPassword &&
                        x.CodeHash == codeHash &&
                        x.UsedAt == null)
                    .OrderByDescending(x => x.CreatedAt)
                    .FirstOrDefaultAsync();

            if (verificationCode is null)
            {
                throw new BadRequestException(
                    "Invalid password reset code");
            }

            if (verificationCode.ExpiresAt <= DateTime.UtcNow)
            {
                throw new BadRequestException(
                    "Password reset code has expired");
            }

            verificationCode.UsedAt = DateTime.UtcNow;

            user.PasswordHash = _passwordHasher.HashPassword(
                user,
                request.NewPassword);

            user.RefreshTokenHash = null;
            user.RefreshTokenExpiryTime = null;

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Password reset successfully for user {UserId}.",
                user.Id);
        }


        public async Task RegisterAsync(RegisterRequest request)
        {
            User? existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (existingUser is not null)
            {
                if (!existingUser.IsEmailVerified)
                {
                    _logger.LogInformation(
                        "Registration attempted for unverified user {UserId}. Redirecting to verification flow.",
                        existingUser.Id);

                    return;
                }

                throw new BadRequestException(
                    "A user with this email already exists");
            }

            User user = new User
            {
                Email = request.Email,
                Role = "Member",
                IsEmailVerified = false
            };

            user.PasswordHash = _passwordHasher.HashPassword(
                user,
                request.Password);

            Member member = new Member
            {
                User = user,
                FullName = request.FullName,
                CreatedAt = DateTime.UtcNow
            };

            user.Member = member;

            await _context.Users.AddAsync(user);

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "User {UserId} registered successfully and is waiting for email verification.",
                user.Id);
        }

        public async Task LogoutAsync(int UserId)
        {
            User? user = await _context.Users.FirstOrDefaultAsync(u=> u.Id == UserId);

            if (user is null)
            {
                _logger.LogWarning(
                    "Logout failed because user {UserId} was not found.",
                    UserId);

                throw new UnauthorizedException("User not found");
            }

            user.RefreshTokenHash = null;
            user.RefreshTokenExpiryTime = null;


            await _context.SaveChangesAsync();

            _logger.LogInformation("User {UserId} logged out successfully.", user.Id);
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            User? user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user is null)
            {
                _logger.LogWarning("Failed login attempt: user not found.");
                throw new UnauthorizedException("Invalid email or password");
            }

            PasswordVerificationResult result =
                _passwordHasher.VerifyHashedPassword(
                    user,
                    user.PasswordHash,
                    request.Password);

            if (result == PasswordVerificationResult.Failed)
            {
                _logger.LogWarning(
                    "Failed login attempt for user {UserId}: invalid password.",
                    user.Id);

                throw new UnauthorizedException("Invalid email or password");
            }

            if (!user.IsEmailVerified)
            {
                _logger.LogWarning(
                    "Failed login attempt for user {UserId}: email not verified.",
                    user.Id);

                throw new UnauthorizedException("Email address is not verified");
            }

            string accessToken = CreateAccesToken(user);
            string refreshToken = CreateRefreshToken();

            user.RefreshTokenHash = TokenHasher.Hash(refreshToken);
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(
                _configuration.GetValue<int>("Jwt:RefreshTokenDays"));

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "User {UserId} logged in successfully.",
                user.Id);

            return new AuthResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
        }

        public async Task<CurrentUserResponse> GetCurrentUserAsync(int userId)
        {
            User? user = await _context.Users.Include(u => u.Member).FirstOrDefaultAsync(u => u.Id == userId);

            if (user is null) throw new NotFoundException("User not found");

            return new CurrentUserResponse
            {
                Id = user.Id,
                Email = user.Email,
                Role = user.Role,
                MemberId= user.Member?.Id,
                FullName = user.Member?.FullName
            };
        }

        public async Task<CurrentUserResponse> UpdateProfileAsync(
    int userId,
    UpdateProfileRequest request)
        {
            User? user = await _context.Users
                .Include(u => u.Member)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user is null)
            {
                throw new NotFoundException(
                    "User not found");
            }

            if (user.Member is null)
            {
                throw new NotFoundException(
                    "Member profile not found");
            }

            bool emailExists =
                await _context.Users.AnyAsync(u =>
                    u.Email == request.Email &&
                    u.Id != userId);

            if (emailExists)
            {
                throw new BadRequestException(
                    "A user with this email already exists");
            }

            user.Email =
                request.Email.Trim();

            user.Member.FullName =
                request.FullName.Trim();

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "User {UserId} updated their profile.",
                user.Id);

            return new CurrentUserResponse
            {
                Id = user.Id,
                Email = user.Email,
                Role = user.Role,
                MemberId = user.Member.Id,
                FullName = user.Member.FullName
            };
        }







        public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request)
        {

            string hashedToken = TokenHasher.Hash(request.RefreshToken);

            User? user = await _context.Users.FirstOrDefaultAsync(u => u.RefreshTokenHash == hashedToken);

            if (user is null)
            {
                _logger.LogWarning("Invalid refresh token attempt");
                throw new UnauthorizedException("Invalid refresh token");
            }

            if (user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                _logger.LogWarning("Refresh token has expired for User {ID}" , user.Id);
                throw new UnauthorizedException("Refresh token has expired");
            }

            string accessToken = CreateAccesToken(user);
            string newRefreshToken = CreateRefreshToken();

            user.RefreshTokenHash = TokenHasher.Hash(newRefreshToken);
            
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(_configuration.GetValue<int>("Jwt:RefreshTokenDays"));

            await _context.SaveChangesAsync();

            _logger.LogInformation("Tokens refreshed successfully for user {UserId}.", user.Id);

            return new AuthResponse
            {
                AccessToken = accessToken,
                RefreshToken = newRefreshToken
            };
        }


        private string CreateRefreshToken()
        {
            byte[] randomBytes = RandomNumberGenerator.GetBytes(64);
            return Convert.ToBase64String(randomBytes);
        }

        private string CreateAccesToken(User user)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            string key = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT key is not configured");
            SymmetricSecurityKey securityKey =   new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            SigningCredentials credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            JwtSecurityToken token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(15),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private static string CreateVerificationCode()
        {
            int code = RandomNumberGenerator.GetInt32(100000, 1000000);

            return code.ToString();
        }
    }
}
