using LibraryApi.Controllers;
using LibraryApi.Data;
using LibraryApi.Models;
using LibraryApi.Models.Status;
using LibraryApi.Services;
using LibraryApi.Services.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;

namespace LibraryApi.Tests.Integration;

public sealed class CustomWebApplicationFactory
    : WebApplicationFactory<AuthController>
{
    public const string JwtKey =
        "LibraryApi.Tests.SuperSecretJwtSigningKey.0123456789.abcdefghijklmnopqrstuvwxyz";

    public string DatabaseName { get; } =
        $"LibraryApiTests-{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.UseSetting("Jwt:Key", JwtKey);
        builder.UseSetting("Jwt:Issuer", "LibraryApi.Tests");
        builder.UseSetting("Jwt:Audience", "LibraryApi.Tests.Clients");
        builder.UseSetting("Jwt:RefreshTokenDays", "7");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["Jwt:Key"] = JwtKey,
                    ["Jwt:Issuer"] = "LibraryApi.Tests",
                    ["Jwt:Audience"] = "LibraryApi.Tests.Clients",
                    ["Jwt:RefreshTokenDays"] = "7"
                });
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<LibraryDbContext>();
            services.RemoveAll<DbContextOptions<LibraryDbContext>>();
            services.RemoveAll<
                IDbContextOptionsConfiguration<LibraryDbContext>>();

            services.AddDbContext<LibraryDbContext>(options =>
            {
                options.UseInMemoryDatabase(DatabaseName);
            });

            services.RemoveAll<IBlobStorageService>();

            Mock<IBlobStorageService> blobStorageMock = new();

            blobStorageMock
                .Setup(service =>
                    service.UploadBookImageAsync(It.IsAny<IFormFile>()))
                .ReturnsAsync(
                    "https://test-storage.local/test-image.jpg");

            blobStorageMock
                .Setup(service =>
                    service.DeleteAsync(It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            services.AddSingleton(blobStorageMock.Object);

            services.TryAddScoped<AdminService>();
        });
    }

    public async Task ResetDatabaseAsync()
    {
        using IServiceScope scope = Services.CreateScope();

        LibraryDbContext context =
            scope.ServiceProvider.GetRequiredService<LibraryDbContext>();

        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();

        context.LoanExtensionRequest.RemoveRange(
            context.LoanExtensionRequest);

        context.LoanRequest.RemoveRange(
            context.LoanRequest);

        context.Loans.RemoveRange(
            context.Loans);

        context.Members.RemoveRange(
            context.Members);

        context.Users.RemoveRange(
            context.Users);

        context.Books.RemoveRange(
            context.Books);

        await context.SaveChangesAsync();
    }

    public async Task<(User User, Member Member)> SeedMemberAsync(
        string email = "member@test.com",
        string password = "Secret123!",
        string fullName = "Test Member",
        bool isEmailVerified = true)
    {
        using IServiceScope scope = Services.CreateScope();

        LibraryDbContext context =
            scope.ServiceProvider.GetRequiredService<LibraryDbContext>();

        IPasswordHasher<User> hasher =
            scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>();

        User user = new()
        {
            Email = email,
            Role = "Member",
            IsEmailVerified = isEmailVerified
        };

        user.PasswordHash =
            hasher.HashPassword(user, password);

        Member member = new()
        {
            FullName = fullName,
            CreatedAt = DateTime.UtcNow,
            User = user
        };

        user.Member = member;

        context.Users.Add(user);

        await context.SaveChangesAsync();

        return (user, member);
    }

    public async Task<User> SeedAdminAsync(
        string email = "admin@test.com",
        string password = "Secret123!",
        bool isEmailVerified = true)
    {
        using IServiceScope scope = Services.CreateScope();

        LibraryDbContext context =
            scope.ServiceProvider.GetRequiredService<LibraryDbContext>();

        IPasswordHasher<User> hasher =
            scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>();

        User admin = new()
        {
            Email = email,
            Role = "Admin",
            IsEmailVerified = isEmailVerified
        };

        admin.PasswordHash =
            hasher.HashPassword(admin, password);

        context.Users.Add(admin);

        await context.SaveChangesAsync();

        return admin;
    }

    public async Task<Book> SeedBookAsync(
        string title = "Dune",
        string author = "Frank Herbert",
        BookStatus status = BookStatus.Available)
    {
        using IServiceScope scope = Services.CreateScope();

        LibraryDbContext context =
            scope.ServiceProvider.GetRequiredService<LibraryDbContext>();

        Book book = new()
        {
            Title = title,
            Author = author,
            Description = "Test book description.",
            Status = status
        };

        context.Books.Add(book);

        await context.SaveChangesAsync();

        return book;
    }

    public async Task<Loan> SeedLoanAsync(
        int bookId,
        int memberId,
        LoanStatus status = LoanStatus.Active,
        DateTime? dueDate = null,
        DateTime? returnDate = null)
    {
        using IServiceScope scope = Services.CreateScope();

        LibraryDbContext context =
            scope.ServiceProvider.GetRequiredService<LibraryDbContext>();

        Loan loan = new()
        {
            BookId = bookId,
            MemberId = memberId,
            LoanDate = DateTime.UtcNow.AddDays(-1),
            DueDate = dueDate ?? DateTime.UtcNow.AddDays(6),
            ReturnDate = returnDate,
            Status = status
        };

        context.Loans.Add(loan);

        await context.SaveChangesAsync();

        return loan;
    }

    public async Task<LoanRequest> SeedLoanRequestAsync(
        int bookId,
        int memberId,
        LoanRequestStatus status = LoanRequestStatus.Pending,
        string loanCode = "LR-ABC123")
    {
        using IServiceScope scope = Services.CreateScope();

        LibraryDbContext context =
            scope.ServiceProvider.GetRequiredService<LibraryDbContext>();

        LoanRequest request = new()
        {
            BookId = bookId,
            MemberId = memberId,
            LoanCode = loanCode,
            RequestDate = DateTime.UtcNow,
            Status = status
        };

        context.LoanRequest.Add(request);

        await context.SaveChangesAsync();

        return request;
    }

    public async Task<LoanExtensionRequest> SeedExtensionRequestAsync(
        int loanId,
        int memberId,
        LoanExtensionStatus status = LoanExtensionStatus.Pending)
    {
        using IServiceScope scope = Services.CreateScope();

        LibraryDbContext context =
            scope.ServiceProvider.GetRequiredService<LibraryDbContext>();

        LoanExtensionRequest request = new()
        {
            LoanId = loanId,
            MemberId = memberId,
            RequestDate = DateTime.UtcNow,
            Status = status
        };

        context.LoanExtensionRequest.Add(request);

        await context.SaveChangesAsync();

        return request;
    }
}