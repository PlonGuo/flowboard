using FlowBoard.Application.Commands.Notifications;
using FlowBoard.Application.Common;
using FlowBoard.Core.Interfaces;
using MediatR;

namespace FlowBoard.Application.Handlers.Notifications;

/// <summary>
/// Handler for marking all notifications as read for a user.
/// </summary>
public class MarkAllNotificationsReadHandler : IRequestHandler<MarkAllNotificationsReadCommand, Result<int>>
{
    private readonly IUnitOfWork _unitOfWork;

    public MarkAllNotificationsReadHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<int>> Handle(
        MarkAllNotificationsReadCommand request,
        CancellationToken cancellationToken)
    {
        // Get all unread notifications for the user
        var notifications = await _unitOfWork.Notifications
            .FindAsync(n => n.UserId == request.UserId && !n.IsRead);

        var unreadList = notifications.ToList();
        var count = unreadList.Count;

        if (count == 0)
        {
            return Result.Success(0);
        }

        var now = DateTime.UtcNow;
        foreach (var notification in unreadList)
        {
            notification.IsRead = true;
            notification.ReadAt = now;
            _unitOfWork.Notifications.Update(notification);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(count);
    }
}
