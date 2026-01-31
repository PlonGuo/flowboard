namespace FlowBoard.Core.Entities;

public class Canvas
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? BoardId { get; set; }
    public int TeamId { get; set; }
    public int CreatedById { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Board? Board { get; set; }
    public Team Team { get; set; } = null!;
    public User CreatedBy { get; set; } = null!;
    public CanvasData? Data { get; set; }
    public ICollection<CanvasOperation> Operations { get; set; } = new List<CanvasOperation>();
}
