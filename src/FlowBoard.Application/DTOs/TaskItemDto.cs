using FlowBoard.Core.Enums;

namespace FlowBoard.Application.DTOs;

/// <summary>
/// Task item data transfer object.
/// </summary>
public record TaskItemDto
{
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public int ColumnId { get; init; }
    public string ColumnName { get; init; } = string.Empty;
    public int Position { get; init; }
    public UserSummaryDto? Assignee { get; init; }
    public TaskPriority Priority { get; init; }
    public DateTime? DueDate { get; init; }
    public UserSummaryDto CreatedBy { get; init; } = null!;
    public int CommentCount { get; init; }
    public byte[] RowVersion { get; init; } = null!;
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}

/// <summary>
/// Task with comments for detail view.
/// </summary>
public record TaskItemDetailDto
{
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public int ColumnId { get; init; }
    public string ColumnName { get; init; } = string.Empty;
    public int BoardId { get; init; }
    public string BoardName { get; init; } = string.Empty;
    public int Position { get; init; }
    public UserSummaryDto? Assignee { get; init; }
    public TaskPriority Priority { get; init; }
    public DateTime? DueDate { get; init; }
    public UserSummaryDto CreatedBy { get; init; } = null!;
    public IReadOnlyList<CommentDto> Comments { get; init; } = Array.Empty<CommentDto>();
    public byte[] RowVersion { get; init; } = null!;
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}

/// <summary>
/// Task summary for list views.
/// </summary>
public record TaskItemSummaryDto
{
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public TaskPriority Priority { get; init; }
    public DateTime? DueDate { get; init; }
    public UserSummaryDto? Assignee { get; init; }
}
