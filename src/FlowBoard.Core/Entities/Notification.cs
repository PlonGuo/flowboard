using FlowBoard.Core.Enums;

namespace FlowBoard.Core.Entities;

public class Notification
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public NotificationType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Message { get; set; }
    public int? RelatedTaskId { get; set; }
    public int? RelatedBoardId { get; set; }
    public bool IsRead { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReadAt { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
    public TaskItem? RelatedTask { get; set; }
    public Board? RelatedBoard { get; set; }
}
