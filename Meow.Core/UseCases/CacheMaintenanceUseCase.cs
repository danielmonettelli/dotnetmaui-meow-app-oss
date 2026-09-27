namespace Meow.Core.UseCases;

/// <summary>
/// Use case for performing cache maintenance operations
/// </summary>
public class CacheMaintenanceUseCase
{
    private readonly ICatCacheRepository _catCacheRepository;
    private readonly IBreedCacheRepository _breedCacheRepository;
    private readonly IFavoriteRepository _favoriteRepository;
    private readonly IConnectivityProvider _connectivity;
    private readonly ICatApiService _catApiService;

    public CacheMaintenanceUseCase(
        ICatCacheRepository catCacheRepository,
        IBreedCacheRepository breedCacheRepository,
        IFavoriteRepository favoriteRepository,
        IConnectivityProvider connectivity,
        ICatApiService catApiService)
    {
        _catCacheRepository = catCacheRepository;
        _breedCacheRepository = breedCacheRepository;
        _favoriteRepository = favoriteRepository;
        _connectivity = connectivity;
        _catApiService = catApiService;
    }

    /// <summary>
    /// Performs periodic cache cleanup and sync
    /// </summary>
    public async Task ExecuteMaintenanceAsync()
    {
        try
        {
            await _catCacheRepository.CleanupExpiredCacheAsync();
            await _breedCacheRepository.CleanupExpiredCacheAsync();

            if (_connectivity.IsConnected)
            {
                await _favoriteRepository.SyncFavoritesAsync(_catApiService);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Cache maintenance error: {ex.Message}");
        }
    }

    /// <summary>
    /// Clears all cached data
    /// </summary>
    public async Task ClearAllCacheAsync()
    {
        try
        {
            await Task.WhenAll(
                _catCacheRepository.ClearCacheAsync(),
                _breedCacheRepository.ClearCacheAsync(),
                _favoriteRepository.ClearFavoritesAsync()
            );
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error clearing cache: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets cache statistics
    /// </summary>
    public async Task<CacheStatistics> GetStatisticsAsync()
    {
        try
        {
            return new CacheStatistics
            {
                CachedBreedsCount = await _breedCacheRepository.GetBreedCountAsync(),
                FavoritesCount = (await _favoriteRepository.GetUserFavoritesAsync()).Count,
                UnsyncedFavoritesCount = await _favoriteRepository.GetUnsyncedCountAsync(),
                LastCacheUpdate = DateTime.UtcNow,
                IsOnline = _connectivity.IsConnected
            };
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error getting statistics: {ex.Message}");
            return new CacheStatistics { IsOnline = _connectivity.IsConnected };
        }
    }
}
