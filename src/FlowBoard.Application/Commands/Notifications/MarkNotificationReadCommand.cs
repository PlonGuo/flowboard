using FlowBoard.Application.Common;
using MediatR;

namespace FlowBoard.Application.Commands.Notifications;

/// <summary>
/// Command to mark a notification as read.
/// </summary>
public record MarkNotificationReadCommand(
    int NotificationId,
    int UserId
) : IRequest<Result>;
