using FlowBoard.Application.Commands.Tasks;
using FluentValidation;

namespace FlowBoard.Application.Validators.Tasks;

/// <summary>
/// Validator for move task command.
/// </summary>
public class MoveTaskCommandValidator : AbstractValidator<MoveTaskCommand>
{
    public MoveTaskCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Task Id must be valid");

        RuleFor(x => x.ToColumnId)
            .GreaterThan(0).WithMessage("ToColumnId must be valid");

        RuleFor(x => x.ToPosition)
            .GreaterThanOrEqualTo(0).WithMessage("ToPosition must be zero or greater");

        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("UserId must be valid");

        RuleFor(x => x.RowVersion)
            .NotEmpty().WithMessage("RowVersion is required for concurrency control");
    }
}
