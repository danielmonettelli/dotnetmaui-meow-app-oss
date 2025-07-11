namespace Meow.Data;

/// <summary>
/// Constants for database configuration and settings
/// </summary>
public static class DatabaseConstants
{
    /// <summary>
    /// Database file name
    /// </summary>
    public const string DatabaseFilename = "meow_cache.db3";

    /// <summary>
    /// SQLite connection flags for optimal performance
    /// </summary>
    public const SQLiteOpenFlags Flags =
        SQLiteOpenFlags.ReadWrite |
        SQLiteOpenFlags.Create |
        SQLiteOpenFlags.SharedCache;

    /// <summary>
    /// Maximum number of cached cats for voting
    /// </summary>
    public const int MaxCachedVotingCats = 30;

    /// <summary>
    /// Cache expiration time in hours for voting cats
    /// </summary>
    public const int VotingCacheExpirationHours = 24;

    /// <summary>
    /// Cache expiration time in days for breed data
    /// </summary>
    public const int BreedCacheExpirationDays = 7;

    /// <summary>
    /// Gets the full database path
    /// </summary>
    public static string DatabasePath =>
        System.IO.Path.Combine(FileSystem.AppDataDirectory, DatabaseFilename);
}