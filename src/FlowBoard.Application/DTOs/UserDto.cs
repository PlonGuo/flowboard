namespace FlowBoard.Application.DTOs;

/// <summary>
/// User data transfer object for API responses.
/// </summary>
public record UserDto
{
    public int Id { get; init; }
    public string Email { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string? AvatarUrl { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? LastLoginAt { get; init; }
}

/// <summary>
/// Minimal user info for embedding in other DTOs.
/// </summary>
public record UserSummaryDto
{
    public int Id { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string? AvatarUrl { get; init; }
}
