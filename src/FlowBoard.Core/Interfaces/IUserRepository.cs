using FlowBoard.Core.Entities;

namespace FlowBoard.Core.Interfaces;

/// <summary>
/// Repository interface for User entity with authentication-specific queries.
/// </summary>
public interface IUserRepository : IRepository<User>
{
    /// <summary>
    /// Gets a user by their email address (case-insensitive).
    /// </summary>
    Task<User?> GetByEmailAsync(string email);

    /// <summary>
    /// Checks if a user with the given email already exists.
    /// </summary>
    Task<bool> EmailExistsAsync(string email);
}
