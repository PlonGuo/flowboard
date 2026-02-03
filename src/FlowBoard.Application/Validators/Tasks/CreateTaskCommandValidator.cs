using FlowBoard.Application.Commands.Tasks;
using FluentValidation;

namespace FlowBoard.Application.Validators.Tasks;

/// <summary>
/// Validator for create task command.
/// </summary>
public class CreateTaskCommandValidator : AbstractValidator<CreateTaskCommand>
{
    public CreateTaskCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Task title is required")
            .MaximumLength(200).WithMessage("Task title must not exceed 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.ColumnId)
            .GreaterThan(0).WithMessage("ColumnId must be valid");

        RuleFor(x => x.Priority)
            .IsInEnum().WithMessage("Priority must be a valid value");

        RuleFor(x => x.AssigneeId)
            .GreaterThan(0).WithMessage("AssigneeId must be valid")
            .When(x => x.AssigneeId.HasValue);

        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("UserId must be valid");
    }
}
