using FlowBoard.Application.Commands.Canvas;
using FluentValidation;

namespace FlowBoard.Application.Validators.Canvas;

public class UpdateCanvasDataCommandValidator : AbstractValidator<UpdateCanvasDataCommand>
{
    public UpdateCanvasDataCommandValidator()
    {
        RuleFor(x => x.CanvasId)
            .GreaterThan(0)
            .WithMessage("CanvasId must be greater than 0");

        RuleFor(x => x.Elements)
            .NotEmpty()
            .WithMessage("Elements is required");

        RuleFor(x => x.UserId)
            .GreaterThan(0)
            .WithMessage("UserId must be greater than 0");
    }
}
