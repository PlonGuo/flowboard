using FlowBoard.Application.Commands.Tasks;
using FluentValidation;

namespace FlowBoard.Application.Validators.Tasks;

/// <summary>
/// Validator for update task command.
/// </summary>
public class UpdateTaskCommandValidator : AbstractValidator<UpdateTaskCommand>
{
    public UpdateTaskCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Task Id must be valid");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Task title is required")
            .MaximumLength(200).WithMessage("Task title must not exceed 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.Priority)
            .IsInEnum().WithMessage("Priority must be a valid value");

        RuleFor(x => x.AssigneeId)
            .GreaterThan(0).WithMessage("AssigneeId must be valid")
            .When(x => x.AssigneeId.HasValue);

        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("UserId must be valid");

        RuleFor(x => x.RowVersion)
            .NotEmpty().WithMessage("RowVersion is required for concurrency control");
    }
}
