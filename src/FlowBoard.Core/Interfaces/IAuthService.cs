using FlowBoard.Core.Entities;

namespace FlowBoard.Core.Interfaces;

/// <summary>
/// Authentication service interface for JWT token management.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Generate a JWT access token for the user.
    /// </summary>
    string GenerateAccessToken(User user);

    /// <summary>
    /// Generate a refresh token.
    /// </summary>
    string GenerateRefreshToken();

    /// <summary>
    /// Validate and extract user ID from a token.
    /// </summary>
    int? ValidateToken(string token);

    /// <summary>
    /// Get token expiration time in minutes.
    /// </summary>
    int GetTokenExpirationMinutes();
}
