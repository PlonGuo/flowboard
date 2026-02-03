using FlowBoard.Application.Common;
using FlowBoard.Application.DTOs;
using MediatR;

namespace FlowBoard.Application.Queries.Notifications;

/// <summary>
/// Query to get notifications for a user.
/// </summary>
public record GetNotificationsQuery(
    int UserId,
    bool IncludeRead = true,
    int Skip = 0,
    int Take = 50
) : IRequest<Result<IEnumerable<NotificationDto>>>;
