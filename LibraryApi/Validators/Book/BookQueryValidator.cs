using FluentValidation;
using LibraryApi.DTOs.Books;

namespace LibraryApi.Validators.Books;

public class BookQueryValidator : AbstractValidator<BookQuery>
{
    public BookQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100);

        RuleFor(x => x.SortBy)
            .Must(x => string.IsNullOrWhiteSpace(x) || x.Equals("title", StringComparison.OrdinalIgnoreCase) || x.Equals("author", StringComparison.OrdinalIgnoreCase))
            .WithMessage("SortBy must be 'title' or 'author'.");
    }
}