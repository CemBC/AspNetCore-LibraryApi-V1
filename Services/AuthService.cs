using LibraryApi.Data;
using LibraryApi.DTOs.Auth;
using Microsoft.AspNetCore.Identity;
using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;
using LibraryApi.Exceptions;
using System.Security.Cryptography;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApi.Services
{
    public class AuthService : IAuthService
    {
        private readonly LibraryDbContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;

        private readonly IConfiguration _configuration;

        public AuthService(LibraryDbContext context, IPasswordHasher<User> passwordHasher , IConfiguration configuration)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _configuration = configuration;
        }
        public async Task RegisterAsync(RegisterRequest request)
        {
            bool emailExists = await _context.Users.AnyAsync(u => u.Email == request.Email);

            if (emailExists)
            {
                throw new BadRequestException("A user with this email already exists");
            }

            User user = new User
            {
                Email = request.Email,
                Role = "Member"
            };
            
            user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);
            
            await _context.Users.AddAsync(user);

            await _context.SaveChangesAsync();
        }

        public async Task LogoutAsync(int UserId)
        {
            User? user = await _context.Users.FirstOrDefaultAsync(u=> u.Id == UserId);
            if(user is null) throw new UnauthorizedException("User not found");

            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = null;

            await _context.SaveChangesAsync();
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            User? user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

            if(user is null)
            {
                throw new UnauthorizedException("Invalid email or password");
            }

            PasswordVerificationResult result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
            if(result == PasswordVerificationResult.Failed)
            {
                throw new UnauthorizedException("Invalid email or password");
            }

            string accessToken = CreateAccesToken(user);

            string refreshToken = CreateRefreshToken();

            user.RefreshToken = refreshToken;

            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(_configuration.GetValue<int>("Jwt:RefreshTokenDays"));

            await _context.SaveChangesAsync();

            return new AuthResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };

        }

        public async Task<CurrentUserResponse> GetCurrentUserAsync(int userId)
        {
            User? user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user is null) throw new NotFoundException("User not found");

            return new CurrentUserResponse
            {
                Id = user.Id,
                Email = user.Email,
                Role = user.Role
            };
        }

        public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request)
        {
            User? user = await _context.Users.FirstOrDefaultAsync(u => u.RefreshToken == request.RefreshToken);

            if(user is null) throw new UnauthorizedException("Invalid refresh token");

            if(user.RefreshTokenExpiryTime <= DateTime.UtcNow) throw new UnauthorizedException("Refresh token has expired");

            string accessToken = CreateAccesToken(user);
            string newRefreshToken = CreateRefreshToken();

            user.RefreshToken = newRefreshToken;
            
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(_configuration.GetValue<int>("Jwt:RefreshTokenDays"));

            await _context.SaveChangesAsync();

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
    }
}
