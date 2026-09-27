namespace Meow.Infrastructure.Persistence;

/// <summary>
/// Constants for database configuration.
/// The database file path is now provided via IDatabasePathProvider (Dependency Inversion).
/// </summary>
public static class DatabaseConfig
{
    public const string DatabaseFilename = "meow_cache.db3";

    public const SQLiteOpenFlags Flags =
        SQLiteOpenFlags.ReadWrite |
        SQLiteOpenFlags.Create |
        SQLiteOpenFlags.SharedCache;

    public const int MaxCachedVotingCats = 30;
    public const int VotingCacheExpirationHours = 24;
    public const int BreedCacheExpirationDays = 7;
}
