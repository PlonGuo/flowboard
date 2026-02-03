using FlowBoard.Application.Commands.Canvas;
using FluentValidation;

namespace FlowBoard.Application.Validators.Canvas;

public class CreateTaskCanvasCommandValidator : AbstractValidator<CreateTaskCanvasCommand>
{
    public CreateTaskCanvasCommandValidator()
    {
        RuleFor(x => x.TaskId)
            .GreaterThan(0)
            .WithMessage("TaskId must be greater than 0");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name is required")
            .MaximumLength(100)
            .WithMessage("Name must not exceed 100 characters");

        RuleFor(x => x.UserId)
            .GreaterThan(0)
            .WithMessage("UserId must be greater than 0");
    }
}
