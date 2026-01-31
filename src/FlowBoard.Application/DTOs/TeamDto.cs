using FlowBoard.Core.Enums;

namespace FlowBoard.Application.DTOs;

/// <summary>
/// Team data transfer object.
/// </summary>
public record TeamDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public UserSummaryDto Owner { get; init; } = null!;
    public int MemberCount { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}

/// <summary>
/// Team member data transfer object.
/// </summary>
public record TeamMemberDto
{
    public int Id { get; init; }
    public int TeamId { get; init; }
    public UserSummaryDto User { get; init; } = null!;
    public TeamRole Role { get; init; }
    public DateTime JoinedAt { get; init; }
}
