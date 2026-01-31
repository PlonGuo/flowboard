namespace FlowBoard.Core.Entities;

public class CanvasData
{
    public int Id { get; set; }
    public int CanvasId { get; set; }
    public string Elements { get; set; } = "[]";  // JSON array of Excalidraw elements
    public string? AppState { get; set; }  // JSON object for Excalidraw app state
    public string? Files { get; set; }  // JSON object for embedded files
    public int Version { get; set; } = 1;
    public int UpdatedById { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Canvas Canvas { get; set; } = null!;
    public User UpdatedBy { get; set; } = null!;
}
