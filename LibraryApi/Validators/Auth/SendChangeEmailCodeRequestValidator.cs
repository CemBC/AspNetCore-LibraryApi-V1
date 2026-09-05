using FluentValidation;
using LibraryApi.DTOs.Auth;

namespace LibraryApi.Validators.Auth;

public class SendChangeEmailCodeRequestValidator
    : AbstractValidator<SendChangeEmailCodeRequest>
{
    public SendChangeEmailCodeRequestValidator()
    {
        RuleFor(x => x.NewEmail)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(200);
    }
}