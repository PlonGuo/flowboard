namespace FlowBoard.Application.DTOs;

/// <summary>
/// Comment data transfer object.
/// </summary>
public record CommentDto
{
    public int Id { get; init; }
    public string Content { get; init; } = string.Empty;
    public int TaskId { get; init; }
    public UserSummaryDto Author { get; init; } = null!;
    public bool IsEdited { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}
