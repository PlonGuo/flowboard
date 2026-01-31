namespace FlowBoard.Application.DTOs;

/// <summary>
/// Column data transfer object.
/// </summary>
public record ColumnDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public int BoardId { get; init; }
    public int Position { get; init; }
    public int? WipLimit { get; init; }
    public IReadOnlyList<TaskItemDto> Tasks { get; init; } = Array.Empty<TaskItemDto>();
    public DateTime CreatedAt { get; init; }
}

/// <summary>
/// Column without tasks for lightweight operations.
/// </summary>
public record ColumnSummaryDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public int Position { get; init; }
    public int? WipLimit { get; init; }
    public int TaskCount { get; init; }
}
