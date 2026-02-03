using FlowBoard.Application.Commands.Notifications;
using FlowBoard.Application.Common;
using FlowBoard.Core.Interfaces;
using MediatR;

namespace FlowBoard.Application.Handlers.Notifications;

/// <summary>
/// Handler for deleting a notification.
/// </summary>
public class DeleteNotificationHandler : IRequestHandler<DeleteNotificationCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteNotificationHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        DeleteNotificationCommand request,
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
            return Result.Failure("You don't have permission to delete this notification");
        }

        _unitOfWork.Notifications.Remove(notification);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
