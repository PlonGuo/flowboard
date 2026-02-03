using FlowBoard.Application.Commands.Notifications;
using FlowBoard.Application.Common;
using FlowBoard.Core.Interfaces;
using MediatR;

namespace FlowBoard.Application.Handlers.Notifications;

/// <summary>
/// Handler for marking a notification as read.
/// </summary>
public class MarkNotificationReadHandler : IRequestHandler<MarkNotificationReadCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;

    public MarkNotificationReadHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        MarkNotificationReadCommand request,
        CancellationToken cancellationToken)
    {
        var notification = await _unitOfWork.Notifications.GetByIdAsync(request.NotificationId);

        if (notification == null)
        {
            return Result.Failure("Notification not found");
        }

        // Verify ownership
        if (notification.UserId != request.UserId)
        {
            return Result.Failure("You don't have permission to modify this notification");
        }

        // Already read
        if (notification.IsRead)
        {
            return Result.Success();
        }

        notification.IsRead = true;
        notification.ReadAt = DateTime.UtcNow;

        _unitOfWork.Notifications.Update(notification);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
