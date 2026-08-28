using FluentValidation;
using LibraryApi.DTOs.Members;

namespace LibraryApi.Validators.Members;

public class MemberQueryValidator : AbstractValidator<MemberQuery>
{
    public MemberQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100);

        RuleFor(x => x.SortBy)
            .Must(x =>string.IsNullOrWhiteSpace(x) ||x.Equals("fullname", StringComparison.OrdinalIgnoreCase) ||x.Equals("email", StringComparison.OrdinalIgnoreCase))
            .WithMessage("SortBy must be 'fullName' or 'email'.");
    }
}