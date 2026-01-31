using FlowBoard.Core.Enums;

namespace FlowBoard.Application.DTOs;

/// <summary>
/// Notification data transfer object.
/// </summary>
public record NotificationDto
{
    public int Id { get; init; }
    public int UserId { get; init; }
    public NotificationType Type { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public int? RelatedTaskId { get; init; }
    public int? RelatedBoardId { get; init; }
    public bool IsRead { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? ReadAt { get; init; }
}
