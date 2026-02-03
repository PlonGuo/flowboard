namespace FlowBoard.Application.DTOs;

/// <summary>
/// Board data transfer object.
/// </summary>
public record BoardDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public int TeamId { get; init; }
    public string TeamName { get; init; } = string.Empty;
    public UserSummaryDto CreatedBy { get; init; } = null!;
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}

/// <summary>
/// Board with columns for detail view.
/// </summary>
public record BoardDetailDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public int TeamId { get; init; }
    public string TeamName { get; init; } = string.Empty;
    public string TeamInviteCode { get; init; } = string.Empty;
    public UserSummaryDto CreatedBy { get; init; } = null!;
    public IReadOnlyList<ColumnDto> Columns { get; init; } = Array.Empty<ColumnDto>();
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}

/// <summary>
/// Board summary for list views.
/// </summary>
public record BoardSummaryDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public int TeamId { get; init; }
    public int TaskCount { get; init; }
    public int ColumnCount { get; init; }
    public DateTime UpdatedAt { get; init; }
}
