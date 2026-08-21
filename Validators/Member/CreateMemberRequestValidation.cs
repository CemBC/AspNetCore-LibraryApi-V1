using FluentValidation;
using LibraryApi.DTOs.Members;

namespace LibraryApi.Validators.Member;

public sealed class CreateMemberRequestValidator
    : AbstractValidator<CreateMemberRequest>
{
    public CreateMemberRequestValidator()
    {
        RuleFor(x => x.FullName)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Member name cannot be empty.")
            .MaximumLength(100)
            .WithMessage("Member name cannot exceed 100 characters.");

        RuleFor(x => x.Email)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Email cannot be empty.")
            .EmailAddress()
            .WithMessage("Email format is invalid.");
    }
}