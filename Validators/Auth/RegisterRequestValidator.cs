using FluentValidation;
using LibraryApi.DTOs.Auth;

namespace LibraryApi.Validators.Auth;

public sealed class RegisterRequestValidator
    : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Email)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Email cannot be empty.")
            .EmailAddress()
            .WithMessage("A valid email address is required.")
            .MaximumLength(200);

        RuleFor(x => x.Password)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Password cannot be empty.")
            .MinimumLength(6)
            .WithMessage("Password must be at least 6 characters long.");
    }
}