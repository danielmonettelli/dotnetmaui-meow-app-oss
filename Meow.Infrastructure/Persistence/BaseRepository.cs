namespace Meow.Infrastructure.Persistence;

/// <summary>
/// Base repository providing shared database access and error handling.
/// Uses IDatabasePathProvider for platform-independent database path resolution (SOLID: DIP).
/// </summary>
public abstract class BaseRepository
{
    private readonly IDatabasePathProvider _pathProvider;
    protected SQLiteAsyncConnection? _database;

    protected BaseRepository(IDatabasePathProvider pathProvider)
    {
        _pathProvider = pathProvider;
    }

    protected async Task<SQLiteAsyncConnection> GetDatabaseAsync()
    {
        if (_database != null)
            return _database;

        _database = new SQLiteAsyncConnection(_pathProvider.DatabasePath, DatabaseConfig.Flags);

        await _database.CreateTableAsync<CachedCatEntity>();
        await _database.CreateTableAsync<CachedBreedEntity>();
        await _database.CreateTableAsync<UserFavoriteEntity>();

        return _database;
    }

    public virtual async ValueTask DisposeAsync()
    {
        if (_database != null)
        {
            await _database.CloseAsync();
            _database = null;
        }
    }

    protected async Task<T> ExecuteSafelyAsync<T>(Func<Task<T>> operation, T defaultValue = default!)
    {
        try
        {
            return await operation();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Database operation failed: {ex.Message}");
            return defaultValue;
        }
    }
}
