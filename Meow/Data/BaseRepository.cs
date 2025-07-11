namespace Meow.Data;

/// <summary>
/// Base repository class providing common SQLite operations
/// </summary>
public abstract class BaseRepository
{
    protected SQLiteAsyncConnection _database;

    /// <summary>
    /// Initializes the database connection
    /// </summary>
    protected async Task<SQLiteAsyncConnection> GetDatabaseAsync()
    {
        if (_database != null)
            return _database;

        _database = new SQLiteAsyncConnection(DatabaseConstants.DatabasePath, DatabaseConstants.Flags);
        
        // Create tables if they don't exist
        await _database.CreateTableAsync<CachedCat>();
        await _database.CreateTableAsync<CachedBreed>();
        await _database.CreateTableAsync<UserFavorite>();

        return _database;
    }

    /// <summary>
    /// Disposes the database connection
    /// </summary>
    public virtual async Task DisposeAsync()
    {
        if (_database != null)
        {
            await _database.CloseAsync();
            _database = null;
        }
    }

    /// <summary>
    /// Checks if there's an active internet connection
    /// </summary>
    protected bool IsConnected => Connectivity.NetworkAccess == NetworkAccess.Internet;

    /// <summary>
    /// Executes a database operation safely with error handling
    /// </summary>
    protected async Task<T> ExecuteSafelyAsync<T>(Func<Task<T>> operation, T defaultValue = default)
    {
        try
        {
            return await operation();
        }
        catch (Exception ex)
        {
            // Log error (you can implement proper logging here)
            System.Diagnostics.Debug.WriteLine($"Database operation failed: {ex.Message}");
            return defaultValue;
        }
    }

    /// <summary>
    /// Cleans up expired cache entries
    /// </summary>
    public async Task CleanupExpiredCacheAsync()
    {
        try
        {
            var database = await GetDatabaseAsync();
            
            // Clean expired voting cats
            var votingExpiry = DateTime.UtcNow.AddHours(-DatabaseConstants.VotingCacheExpirationHours);
            await database.ExecuteAsync(
                "DELETE FROM CachedCats WHERE CacheType = ? AND CachedAt < ?", 
                CacheTypes.Voting, votingExpiry);

            // Clean expired breeds (if needed)
            var breedExpiry = DateTime.UtcNow.AddDays(-DatabaseConstants.BreedCacheExpirationDays);
            await database.ExecuteAsync(
                "DELETE FROM CachedBreeds WHERE CachedAt < ?", 
                breedExpiry);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Cache cleanup failed: {ex.Message}");
        }
    }
}