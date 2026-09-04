using FluentValidation;
using LibraryApi.DTOs.Auth;

namespace LibraryApi.Validators.Auth;

public class SendChangePasswordCodeRequestValidator
    : AbstractValidator<SendChangePasswordCodeRequest>
{
    public SendChangePasswordCodeRequestValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty()
            .WithMessage("Current password is required.");
    }
}