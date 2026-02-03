using FlowBoard.Application.Common;
using MediatR;

namespace FlowBoard.Application.Commands.Notifications;

/// <summary>
/// Command to delete a notification.
/// </summary>
public record DeleteNotificationCommand(
    int NotificationId,
    int UserId
) : IRequest<Result>;
