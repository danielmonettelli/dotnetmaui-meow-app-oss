namespace Meow.Domain.Interfaces;

/// <summary>
/// Provides a unique user or installation identifier for segmenting user-specific data (e.g. favorites sync).
/// </summary>
public interface IUserIdentifierProvider
{
    /// <summary>
    /// Gets the unique identifier for the current installation or user.
    /// </summary>
    string GetUserIdentifier();
}
