using FlowBoard.Application.Commands.Auth;
using FluentValidation;

namespace FlowBoard.Application.Validators.Auth;

/// <summary>
/// Validator for login command.
/// </summary>
public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required");
    }
}
