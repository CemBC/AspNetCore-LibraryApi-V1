using FluentValidation;
using LibraryApi.DTOs.Loans;

namespace LibraryApi.Validators.Loan;

public sealed class CreateLoanRequestValidator
    : AbstractValidator<CreateLoanRequest>
{
    public CreateLoanRequestValidator()
    {
        RuleFor(x => x.BookId)
            .GreaterThan(0)
            .WithMessage("Book ID must be greater than 0");


        RuleFor(x => x.MemberId)
            .GreaterThan(0)
            .WithMessage("Member ID must be greater than 0");
    }
}