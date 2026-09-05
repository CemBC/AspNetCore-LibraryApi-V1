using FluentValidation;
using LibraryApi.Validators.Auth;
using LibraryApi.Validators.Book;
using LibraryApi.Validators.Books;
using LibraryApi.Validators.Loan;
using LibraryApi.Validators.Loans;
using LibraryApi.Validators.LoanExtensions;
using LibraryApi.Validators.Member;
using LibraryApi.Validators.Members;
using LibraryApi.DTOs.Books;
using LibraryApi.DTOs.Members;
using LibraryApi.DTOs.Loans;
using LibraryApi.DTOs.LoanRequests;
using LibraryApi.DTOs.LoanExtensions;
using LibraryApi.DTOs.Auth;

namespace LibraryApi.Tests.Unit;

public class ValidatorTests
{
    [Theory]
    [InlineData(0, 10)]
    [InlineData(-1, 10)]
    [InlineData(1, 0)]
    [InlineData(1, 101)]
    public void BookQueryValidator_InvalidPagination_Fails(int page, int pageSize)
    {
        BookQueryValidator validator = new();

        var result = validator.Validate(new BookQuery
        {
            Page = page,
            PageSize = pageSize
        });

        Assert.False(result.IsValid);
    }

    [Fact]
    public void BookQueryValidator_InvalidSortBy_Fails()
    {
        BookQueryValidator validator = new();
        var result = validator.Validate(new BookQuery { SortBy = "id" });

        Assert.False(result.IsValid);
    }

    [Fact]
    public void MemberQueryValidator_InvalidSortBy_Fails()
    {
        MemberQueryValidator validator = new();
        var result = validator.Validate(new MemberQuery { SortBy = "createdAt" });

        Assert.False(result.IsValid);
    }

    [Fact]
    public void LoanQueryValidator_InvalidSortBy_Fails()
    {
        LoanQueryValidator validator = new();
        var result = validator.Validate(new LoanQuery { SortBy = "status" });

        Assert.False(result.IsValid);
    }

    [Fact]
    public void LoanRequestQueryValidator_InvalidSortBy_Fails()
    {
        LoanRequestQueryValidator validator = new();
        var result = validator.Validate(new LoanRequestQuery { SortBy = "loanCode" });

        Assert.False(result.IsValid);
    }

    [Fact]
    public void LoanExtensionRequestQueryValidator_InvalidSortBy_Fails()
    {
        LoanExtensionRequestQueryValidator validator = new();
        var result = validator.Validate(new LoanExtensionRequestQuery { SortBy = "loanId" });

        Assert.False(result.IsValid);
    }

    [Fact]
    public void RegisterRequestValidator_ValidRequest_Passes()
    {
        RegisterRequestValidator validator = new();

        var result = validator.Validate(new RegisterRequest
        {
            Email = "valid@test.com",
            Password = "Secret123!",
            FullName = "Cem Basar"
        });

        Assert.True(result.IsValid);
    }

    [Fact]
    public void RegisterRequestValidator_NameWithDigits_Fails()
    {
        RegisterRequestValidator validator = new();

        var result = validator.Validate(new RegisterRequest
        {
            Email = "valid@test.com",
            Password = "Secret123!",
            FullName = "Cem123"
        });

        Assert.False(result.IsValid);
    }

    [Fact]
    public void LoginRequestValidator_InvalidEmailAndEmptyPassword_Fails()
    {
        LoginRequestValidator validator = new();

        var result = validator.Validate(new LoginRequest
        {
            Email = "not-an-email",
            Password = ""
        });

        Assert.False(result.IsValid);
        Assert.True(result.Errors.Count >= 2);
    }

    [Fact]
    public void CreateBookRequestValidator_EmptyValues_Fails()
    {
        CreateBookRequestValidator validator = new();

        var result = validator.Validate(new CreateBookRequest
        {
            Title = "",
            Author = ""
        });

        Assert.False(result.IsValid);
    }

    [Fact]
    public void UpdateBookRequestValidator_ValidValues_Passes()
    {
        UpdateBookRequestValidator validator = new();

        var result = validator.Validate(new UpdateBookRequest
        {
            Title = "Dune",
            Author = "Frank Herbert",
            Description = "Science fiction novel."
        });

        Assert.True(result.IsValid);
    }

    [Fact]
    public void CreateLoanRequestValidator_NonPositiveIds_Fails()
    {
        CreateLoanRequestValidator validator = new();

        var result = validator.Validate(new CreateLoanRequest
        {
            BookId = 0,
            MemberId = -1
        });

        Assert.False(result.IsValid);
        Assert.Equal(2, result.Errors.Count);
    }

    [Fact]
    public void CreateMemberRequestValidator_InvalidEmail_Fails()
    {
        CreateMemberRequestValidator validator = new();

        var result = validator.Validate(new CreateMemberRequest
        {
            FullName = "Test Member",
            Email = "invalid"
        });

        Assert.False(result.IsValid);
    }

    [Fact]
    public void UpdateMemberRequestValidator_EmptyName_Fails()
    {
        UpdateMemberRequestValidator validator = new();

        var result = validator.Validate(new UpdateMemberRequest
        {
            FullName = "",
            Email = "valid@test.com"
        });

        Assert.False(result.IsValid);
    }
}
