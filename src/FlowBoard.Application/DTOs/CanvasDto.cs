namespace FlowBoard.Application.DTOs;

/// <summary>
/// Canvas data transfer object.
/// </summary>
public record CanvasDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public int? BoardId { get; init; }
    public int? TeamId { get; init; }
    public UserSummaryDto CreatedBy { get; init; } = null!;
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}

/// <summary>
/// Canvas with data for editing.
/// </summary>
public record CanvasDetailDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public int? BoardId { get; init; }
    public int? TeamId { get; init; }
    public UserSummaryDto CreatedBy { get; init; } = null!;
    public CanvasDataDto? Data { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}

/// <summary>
/// Canvas data (Excalidraw format).
/// </summary>
public record CanvasDataDto
{
    public int Id { get; init; }
    public int CanvasId { get; init; }
    public string Elements { get; init; } = "[]";
    public string? AppState { get; init; }
    public string? Files { get; init; }
    public int Version { get; init; }
    public DateTime UpdatedAt { get; init; }
}
