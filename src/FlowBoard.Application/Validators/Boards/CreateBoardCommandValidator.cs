using FlowBoard.Application.Commands.Boards;
using FluentValidation;

namespace FlowBoard.Application.Validators.Boards;

/// <summary>
/// Validator for create board command.
/// </summary>
public class CreateBoardCommandValidator : AbstractValidator<CreateBoardCommand>
{
    public CreateBoardCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Board name is required")
            .MaximumLength(100).WithMessage("Board name must not exceed 100 characters");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.TeamId)
            .GreaterThan(0).WithMessage("TeamId must be valid");

        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("UserId must be valid");
    }
}
