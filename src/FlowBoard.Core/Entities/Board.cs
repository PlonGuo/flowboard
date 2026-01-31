namespace FlowBoard.Core.Entities;

public class Board
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int TeamId { get; set; }
    public int CreatedById { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Team Team { get; set; } = null!;
    public User CreatedBy { get; set; } = null!;
    public ICollection<Column> Columns { get; set; } = new List<Column>();
    public ICollection<Canvas> Canvases { get; set; } = new List<Canvas>();
    public ICollection<ActivityLog> ActivityLogs { get; set; } = new List<ActivityLog>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    public ICollection<AIChatHistory> ChatHistory { get; set; } = new List<AIChatHistory>();
}
