using FluentValidation;
using LibraryApi.DTOs.LoanRequests;

public class LoanRequestQueryValidator
    : AbstractValidator<LoanRequestQuery>
{
    public LoanRequestQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100);

        RuleFor(x => x.SortBy).Must(x =>string.IsNullOrWhiteSpace(x) ||x.Equals("requestDate", StringComparison.OrdinalIgnoreCase) ||x.Equals("bookTitle", StringComparison.OrdinalIgnoreCase) || x.Equals("memberName", StringComparison.OrdinalIgnoreCase))
            .WithMessage("SortBy must be 'requestDate', 'bookTitle' or 'memberName'.");
    }
}