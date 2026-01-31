namespace FlowBoard.Core.Interfaces;

/// <summary>
/// Password hashing service interface.
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// Hash a password.
    /// </summary>
    string Hash(string password);

    /// <summary>
    /// Verify a password against a hash.
    /// </summary>
    bool Verify(string password, string hash);
}
