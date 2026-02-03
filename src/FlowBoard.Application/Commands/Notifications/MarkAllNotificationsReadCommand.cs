using FlowBoard.Application.Common;
using MediatR;

namespace FlowBoard.Application.Commands.Notifications;

/// <summary>
/// Command to mark all notifications as read for a user.
/// </summary>
public record MarkAllNotificationsReadCommand(
    int UserId
) : IRequest<Result<int>>;
