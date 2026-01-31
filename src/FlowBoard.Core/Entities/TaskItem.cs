using System.ComponentModel.DataAnnotations;
using FlowBoard.Core.Enums;

namespace FlowBoard.Core.Entities;

/// <summary>
/// Task entity for kanban board tasks.
/// Named TaskItem to avoid conflict with System.Threading.Tasks.Task.
/// </summary>
public class TaskItem
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int ColumnId { get; set; }
    public int Position { get; set; }
    public int? AssigneeId { get; set; }
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;
    public DateTime? DueDate { get; set; }
    public int CreatedById { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [Timestamp]
    public byte[] RowVersion { get; set; } = null!;

    // Navigation properties
    public Column Column { get; set; } = null!;
    public User? Assignee { get; set; }
    public User CreatedBy { get; set; } = null!;
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    /// <summary>
    /// Move task to a new column and position.
    /// </summary>
    public void Move(int newColumnId, int newPosition)
    {
        ColumnId = newColumnId;
        Position = newPosition;
        UpdatedAt = DateTime.UtcNow;
    }
}
