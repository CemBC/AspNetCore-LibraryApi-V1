using FluentValidation;
using LibraryApi.DTOs.Auth;

namespace LibraryApi.Validators.Auth;

public class VerifyEmailRequestValidator
    : AbstractValidator<VerifyEmailRequest>
{
    public VerifyEmailRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.Code)
            .NotEmpty()
            .Length(6)
            .Matches(@"^\d{6}$")
            .WithMessage("Verification code must contain 6 digits.");
    }
}