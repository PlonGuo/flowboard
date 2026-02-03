using FlowBoard.Application.Common;
using MediatR;

namespace FlowBoard.Application.Queries.Notifications;

/// <summary>
/// Query to get the count of unread notifications for a user.
/// </summary>
public record GetUnreadNotificationCountQuery(
    int UserId
) : IRequest<Result<int>>;
