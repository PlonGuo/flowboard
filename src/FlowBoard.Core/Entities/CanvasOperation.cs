namespace FlowBoard.Core.Entities;

public class CanvasOperation
{
    public int Id { get; set; }
    public int CanvasId { get; set; }
    public int UserId { get; set; }
    public string OperationType { get; set; } = string.Empty;
    public string ElementId { get; set; } = string.Empty;
    public string OperationData { get; set; } = string.Empty;  // JSON
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Canvas Canvas { get; set; } = null!;
    public User User { get; set; } = null!;
}
