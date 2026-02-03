using FlowBoard.Application.Commands.Teams;
using FluentValidation;

namespace FlowBoard.Application.Validators.Teams;

public class DeleteTeamCommandValidator : AbstractValidator<DeleteTeamCommand>
{
    public DeleteTeamCommandValidator()
    {
        RuleFor(x => x.TeamId)
            .GreaterThan(0).WithMessage("Team ID must be greater than 0");

        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("User ID must be greater than 0");
    }
}
