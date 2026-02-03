using FlowBoard.Application.Commands.Notifications;
using FluentValidation;

namespace FlowBoard.Application.Validators.Notifications;

/// <summary>
/// Validator for mark notification read command.
/// </summary>
public class MarkNotificationReadCommandValidator : AbstractValidator<MarkNotificationReadCommand>
{
    public MarkNotificationReadCommandValidator()
    {
        RuleFor(x => x.NotificationId)
            .GreaterThan(0).WithMessage("NotificationId must be valid");

        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("UserId must be valid");
    }
}
