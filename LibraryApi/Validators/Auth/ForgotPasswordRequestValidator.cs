using FluentValidation;
using LibraryApi.DTOs.Auth;

namespace LibraryApi.Validators.Auth;

public class ForgotPasswordRequestValidator
    : AbstractValidator<ForgotPasswordRequest>
{
    public ForgotPasswordRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();
    }
}