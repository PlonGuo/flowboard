namespace FlowBoard.Application.DTOs;

/// <summary>
/// Authentication response with token.
/// </summary>
public record AuthResponseDto
{
    public string Token { get; init; } = string.Empty;
    public string RefreshToken { get; init; } = string.Empty;
    public DateTime ExpiresAt { get; init; }
    public UserDto User { get; init; } = null!;
}

/// <summary>
/// Token refresh response.
/// </summary>
public record TokenRefreshResponseDto
{
    public string Token { get; init; } = string.Empty;
    public string RefreshToken { get; init; } = string.Empty;
    public DateTime ExpiresAt { get; init; }
}
