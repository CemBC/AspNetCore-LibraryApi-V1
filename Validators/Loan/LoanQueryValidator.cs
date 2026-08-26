using FluentValidation;
using LibraryApi.DTOs.Loans;

namespace LibraryApi.Validators.Loans;

public class LoanQueryValidator : AbstractValidator<LoanQuery>
{
    public LoanQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100);

        RuleFor(x => x.SortBy)
            .Must(x =>
                string.IsNullOrWhiteSpace(x) ||
                x.Equals("loanDate", StringComparison.OrdinalIgnoreCase) ||
                x.Equals("dueDate", StringComparison.OrdinalIgnoreCase) ||
                x.Equals("bookTitle", StringComparison.OrdinalIgnoreCase) ||
                x.Equals("memberName", StringComparison.OrdinalIgnoreCase))
            .WithMessage(
                "SortBy must be 'loanDate', 'dueDate', 'bookTitle' or 'memberName'.");
    }
}