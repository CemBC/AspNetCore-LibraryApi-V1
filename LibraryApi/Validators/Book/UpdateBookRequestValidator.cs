using FluentValidation;
using LibraryApi.DTOs.Books;

namespace LibraryApi.Validators.Book;

public sealed class UpdateBookRequestValidator : AbstractValidator<UpdateBookRequest>
{
    public UpdateBookRequestValidator()
    {
        RuleFor(x => x.Title)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Book title cannot be empty.")
            .MaximumLength(100)
            .WithMessage("Book title cannot exceed 100 characters.");

        RuleFor(x => x.Author)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Book author cannot be empty.")
            .MaximumLength(100)
            .WithMessage("Book author cannot exceed 100 characters.");

        RuleFor(x => x.Description)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Book description cannot be empty.")
            .MaximumLength(1000)
            .WithMessage("Book description cannot exceed 1000 characters.");
    }
}