using AutoMapper;
using LibraryApi.Data;
using LibraryApi.Mappings;
using LibraryApi.Models;
using LibraryApi.Models.Status;
using LibraryApi.Services;
using LibraryApi.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace LibraryApi.Tests.Helpers;

internal static class TestServiceFactory
{
    public static LibraryDbContext CreateContext()
    {
        DbContextOptions<LibraryDbContext> options =
            new DbContextOptionsBuilder<LibraryDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

        LibraryDbContext context = new(options);
        context.Database.EnsureCreated();

        if (context.Books.Any())
        {
            context.Books.RemoveRange(context.Books);
            context.SaveChanges();
        }

        return context;
    }

    public static IMapper CreateMapper()
    {
        ServiceCollection services = new();

        services.AddLogging();
        services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());

        ServiceProvider provider = services.BuildServiceProvider();

        return provider.GetRequiredService<IMapper>();
    }

    public static IConfiguration CreateConfiguration()
    {
        Dictionary<string, string?> values = new()
        {
            ["Jwt:Key"] =
                "LibraryApi.Tests.SuperSecretJwtSigningKey.0123456789.abcdefghijklmnopqrstuvwxyz",
            ["Jwt:Issuer"] = "LibraryApi.Tests",
            ["Jwt:Audience"] = "LibraryApi.Tests.Clients",
            ["Jwt:RefreshTokenDays"] = "7"
        };

        return new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();
    }

    public static async Task<(User User, Member Member)> AddMemberAsync(
    LibraryDbContext context,
    string email = "member@test.com",
    string fullName = "Test Member",
    string password = "Secret123!",
    string role = "Member",
    bool isEmailVerified = true)
    {
        PasswordHasher<User> hasher = new();

        User user = new()
        {
            Email = email,
            Role = role,
            IsEmailVerified = isEmailVerified
        };

        user.PasswordHash = hasher.HashPassword(user, password);

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

    public static async Task<User> AddAdminAsync(
        LibraryDbContext context,
        string email = "admin@test.com",
        string password = "Secret123!")
    {
        PasswordHasher<User> hasher = new();

        User admin = new()
        {
            Email = email,
            Role = "Admin"
        };

        admin.PasswordHash = hasher.HashPassword(admin, password);

        context.Users.Add(admin);
        await context.SaveChangesAsync();

        return admin;
    }

    public static Book AddBook(
        LibraryDbContext context,
        string title = "Dune",
        string author = "Frank Herbert",
        BookStatus status = BookStatus.Available)
    {
        Book book = new()
        {
            Title = title,
            Author = author,
            Description = "Test book description.",
            Status = status
        };

        context.Books.Add(book);
        context.SaveChanges();

        return book;
    }

    public static Loan AddLoan(
        LibraryDbContext context,
        Book book,
        Member member,
        LoanStatus status = LoanStatus.Active,
        DateTime? dueDate = null,
        DateTime? returnDate = null)
    {
        Loan loan = new()
        {
            BookId = book.Id,
            MemberId = member.Id,
            Book = book,
            Member = member,
            LoanDate = DateTime.UtcNow.AddDays(-1),
            DueDate = dueDate ?? DateTime.UtcNow.AddDays(6),
            ReturnDate = returnDate,
            Status = status
        };

        context.Loans.Add(loan);
        context.SaveChanges();

        return loan;
    }

    public static LoanRequest AddLoanRequest(
        LibraryDbContext context,
        Book book,
        Member member,
        LoanRequestStatus status = LoanRequestStatus.Pending,
        string? code = null,
        DateTime? requestDate = null)
    {
        LoanRequest request = new()
        {
            BookId = book.Id,
            MemberId = member.Id,
            Book = book,
            Member = member,
            LoanCode =
                code ??
                $"LR-{Guid.NewGuid():N}"[..9].ToUpperInvariant(),
            RequestDate = requestDate ?? DateTime.UtcNow,
            Status = status
        };

        context.LoanRequest.Add(request);
        context.SaveChanges();

        return request;
    }

    public static LoanExtensionRequest AddExtensionRequest(
        LibraryDbContext context,
        Loan loan,
        Member member,
        LoanExtensionStatus status = LoanExtensionStatus.Pending,
        DateTime? requestDate = null)
    {
        LoanExtensionRequest request = new()
        {
            LoanId = loan.Id,
            MemberId = member.Id,
            Loan = loan,
            Member = member,
            RequestDate = requestDate ?? DateTime.UtcNow,
            Status = status
        };

        context.LoanExtensionRequest.Add(request);
        context.SaveChanges();

        return request;
    }

    public static AuthService CreateAuthService(
        LibraryDbContext context)
    {
        Mock<IEmailService> emailServiceMock = new();

        return new AuthService(
            context,
            new PasswordHasher<User>(),
            CreateConfiguration(),
            NullLogger<AuthService>.Instance,
            emailServiceMock.Object);
    }

    public static BookService CreateBookService(
        LibraryDbContext context)
    {
        Mock<IBlobStorageService> blobStorageServiceMock = new();

        return new BookService(
            context,
            CreateMapper(),
            NullLogger<BookService>.Instance,
            blobStorageServiceMock.Object);
    }

    public static MemberService CreateMemberService(
        LibraryDbContext context)
    {
        Mock<IEmailService> emailServiceMock = new();

        return new MemberService(
            context,
            CreateMapper(),
            NullLogger<MemberService>.Instance,
            new PasswordHasher<User>(),
            emailServiceMock.Object);
    }

    public static LoanService CreateLoanService(
        LibraryDbContext context)
    {
        return new LoanService(
            context,
            CreateMapper(),
            NullLogger<LoanService>.Instance);
    }

    public static LoanRequestService CreateLoanRequestService(
        LibraryDbContext context)
    {
        LoanService loanService = CreateLoanService(context);

        return new LoanRequestService(
            context,
            CreateMapper(),
            loanService,
            NullLogger<LoanRequestService>.Instance);
    }

    public static LoanExtensionService CreateLoanExtensionService(
        LibraryDbContext context)
    {
        return new LoanExtensionService(
            context,
            CreateMapper(),
            NullLogger<LoanExtensionService>.Instance);
    }
}