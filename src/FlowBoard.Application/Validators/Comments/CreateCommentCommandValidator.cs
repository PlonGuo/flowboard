using FlowBoard.Application.Commands.Comments;
using FluentValidation;

namespace FlowBoard.Application.Validators.Comments;

/// <summary>
/// Validator for create comment command.
/// </summary>
public class CreateCommentCommandValidator : AbstractValidator<CreateCommentCommand>
{
    public CreateCommentCommandValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Comment content is required")
            .MaximumLength(5000).WithMessage("Comment must not exceed 5000 characters");

        RuleFor(x => x.TaskId)
            .GreaterThan(0).WithMessage("TaskId must be valid");

        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("UserId must be valid");
    }
}
