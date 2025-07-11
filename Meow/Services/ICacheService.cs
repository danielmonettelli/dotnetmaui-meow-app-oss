namespace Meow.Services;

/// <summary>
/// Interface for cache management service
/// </summary>
public interface ICacheService
{
    #region Cat Caching

    /// <summary>
    /// Gets cached cats for voting with fallback to API
    /// </summary>
    Task<List<Cat>> GetVotingCatsAsync(bool forceRefresh = false);

    /// <summary>
    /// Gets cats by breed with caching
    /// </summary>
    Task<List<Cat>> GetCatsByBreedAsync(string breedId, bool forceRefresh = false);

    #endregion

    #region Breed Caching

    /// <summary>
    /// Gets all breeds with caching
    /// </summary>
    Task<List<Breed>> GetBreedsAsync(bool forceRefresh = false);

    /// <summary>
    /// Gets a specific breed by ID
    /// </summary>
    Task<Breed> GetBreedByIdAsync(string breedId);

    /// <summary>
    /// Searches breeds by term
    /// </summary>
    Task<List<Breed>> SearchBreedsAsync(string searchTerm);

    #endregion

    #region Favorites Management

    /// <summary>
    /// Gets user favorites with local persistence
    /// </summary>
    Task<List<FavoriteCatResponse>> GetFavoritesAsync();

    /// <summary>
    /// Adds a cat to favorites
    /// </summary>
    Task<bool> AddFavoriteAsync(Cat cat);

    /// <summary>
    /// Removes a cat from favorites
    /// </summary>
    Task<bool> RemoveFavoriteAsync(string imageId);

    /// <summary>
    /// Checks if a cat is favorited
    /// </summary>
    Task<bool> IsFavoriteAsync(string imageId);

    /// <summary>
    /// Syncs favorites with server
    /// </summary>
    Task<bool> SyncFavoritesAsync();

    /// <summary>
    /// Gets count of unsynced favorites
    /// </summary>
    Task<int> GetUnsyncedFavoritesCountAsync();

    #endregion

    #region Cache Management

    /// <summary>
    /// Clears all cached data
    /// </summary>
    Task ClearAllCacheAsync();

    /// <summary>
    /// Performs background cache maintenance
    /// </summary>
    Task PerformCacheMaintenanceAsync();

    /// <summary>
    /// Gets cache statistics
    /// </summary>
    Task<CacheStatistics> GetCacheStatisticsAsync();

    #endregion
}

/// <summary>
/// Cache statistics model
/// </summary>
public class CacheStatistics
{
    public int CachedCatsCount { get; set; }
    public int CachedBreedsCount { get; set; }
    public int FavoritesCount { get; set; }
    public int UnsyncedFavoritesCount { get; set; }
    public DateTime LastCacheUpdate { get; set; }
    public long CacheSizeBytes { get; set; }
    public bool IsOnline { get; set; }
}
