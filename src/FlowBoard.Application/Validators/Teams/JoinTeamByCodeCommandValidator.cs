using FlowBoard.Application.Commands.Teams;
using FluentValidation;

namespace FlowBoard.Application.Validators.Teams;

public class JoinTeamByCodeCommandValidator : AbstractValidator<JoinTeamByCodeCommand>
{
    public JoinTeamByCodeCommandValidator()
    {
        RuleFor(x => x.InviteCode)
            .NotEmpty().WithMessage("Invite code is required")
            .Length(8).WithMessage("Invite code must be 8 characters");

        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("User ID must be greater than 0");
    }
}
