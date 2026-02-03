using FlowBoard.Application.Common;
using FlowBoard.Application.Queries.Notifications;
using FlowBoard.Core.Interfaces;
using MediatR;

namespace FlowBoard.Application.Handlers.Notifications;

/// <summary>
/// Handler for getting the count of unread notifications for a user.
/// </summary>
public class GetUnreadNotificationCountHandler : IRequestHandler<GetUnreadNotificationCountQuery, Result<int>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetUnreadNotificationCountHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<int>> Handle(
        GetUnreadNotificationCountQuery request,
        CancellationToken cancellationToken)
    {
        var count = await _unitOfWork.Notifications
            .CountAsync(n => n.UserId == request.UserId && !n.IsRead);

        return Result.Success(count);
    }
}
