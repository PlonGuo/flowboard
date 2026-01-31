namespace FlowBoard.Core.Entities;

public class ActivityLog
{
    public int Id { get; set; }
    public int BoardId { get; set; }
    public int UserId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public int EntityId { get; set; }
    public string? Details { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Board Board { get; set; } = null!;
    public User User { get; set; } = null!;
}
