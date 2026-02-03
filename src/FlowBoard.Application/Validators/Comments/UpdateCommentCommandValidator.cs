using FlowBoard.Application.Commands.Comments;
using FluentValidation;

namespace FlowBoard.Application.Validators.Comments;

/// <summary>
/// Validator for update comment command.
/// </summary>
public class UpdateCommentCommandValidator : AbstractValidator<UpdateCommentCommand>
{
    public UpdateCommentCommandValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Comment content is required")
            .MaximumLength(5000).WithMessage("Comment must not exceed 5000 characters");

        RuleFor(x => x.CommentId)
            .GreaterThan(0).WithMessage("CommentId must be valid");

        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("UserId must be valid");
    }
}
