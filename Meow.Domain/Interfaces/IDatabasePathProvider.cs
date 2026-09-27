namespace Meow.Domain.Interfaces;

/// <summary>
/// Abstraction for providing database file path.
/// Implements Dependency Inversion Principle: Infrastructure depends on this abstraction
/// instead of directly using platform-specific APIs (e.g., MAUI's FileSystem.AppDataDirectory).
/// Each platform provides its own implementation.
/// </summary>
public interface IDatabasePathProvider
{
    /// <summary>
    /// Gets the full path to the SQLite database file
    /// </summary>
    string DatabasePath { get; }
}
