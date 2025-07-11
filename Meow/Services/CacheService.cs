namespace Meow.Services;

/// <summary>
/// Cache service implementation providing intelligent caching with offline support
/// </summary>
public class CacheService : ICacheService
{
    #region Private Fields

    private readonly ICatService _catService;
    private readonly CatCacheRepository _catCacheRepository;
    private readonly BreedCacheRepository _breedCacheRepository;
    private readonly FavoriteRepository _favoriteRepository;

    #endregion

    #region Constructor

    public CacheService(ICatService catService)
    {
        _catService = catService;
        _catCacheRepository = new CatCacheRepository();
        _breedCacheRepository = new BreedCacheRepository();
        _favoriteRepository = new FavoriteRepository();

        // Perform background maintenance on startup
        _ = Task.Run(PerformCacheMaintenanceAsync);
    }

    #endregion

    #region Cat Caching

    public async Task<List<Cat>> GetVotingCatsAsync(bool forceRefresh = false)
    {
        try
        {
            // Check if we should use cache or refresh
            var shouldRefresh = forceRefresh || await _catCacheRepository.ShouldRefreshVotingCacheAsync();
            
            if (shouldRefresh && Connectivity.NetworkAccess == NetworkAccess.Internet)
            {
                // Try to get fresh data from API
                var freshCats = await _catService.GetRandomKitty();
                if (freshCats?.Any() == true)
                {
                    // Cache the new cats
                    await _catCacheRepository.CacheVotingCatsAsync(freshCats);
                    
                    // Return fresh data plus some cached cats for better UX
                    var cachedCats = await _catCacheRepository.GetCachedVotingCatsAsync(10);
                    var combinedCats = freshCats.Concat(cachedCats).Distinct().Take(15).ToList();
                    return combinedCats;
                }
            }

            // Fall back to cached data
            var cachedData = await _catCacheRepository.GetCachedVotingCatsAsync(15);
            
            // If no cached data and no internet, return empty list
            if (!cachedData.Any() && Connectivity.NetworkAccess != NetworkAccess.Internet)
            {
                return new List<Cat>();
            }

            // If we have some cached data but no internet, return what we have
            if (cachedData.Any())
            {
                return cachedData;
            }

            // Last resort: try API even if it might fail
            return await _catService.GetRandomKitty() ?? new List<Cat>();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in GetVotingCatsAsync: {ex.Message}");
            
            // Always try to return cached data on error
            return await _catCacheRepository.GetCachedVotingCatsAsync(10);
        }
    }

    public async Task<List<Cat>> GetCatsByBreedAsync(string breedId, bool forceRefresh = false)
    {
        try
        {
            // Check cache first
            var cachedCats = await _catCacheRepository.GetCachedCatsByBreedAsync(breedId, 10);
            
            if (!forceRefresh && cachedCats.Any())
            {
                return cachedCats;
            }

            // Try to get fresh data if online
            if (Connectivity.NetworkAccess == NetworkAccess.Internet)
            {
                var freshCats = await _catService.GetRandomKittensByBreed(breedId);
                if (freshCats?.Any() == true)
                {
                    await _catCacheRepository.CacheCatsByBreedAsync(freshCats, breedId);
                    return freshCats;
                }
            }

            // Return cached data if available
            return cachedCats;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in GetCatsByBreedAsync: {ex.Message}");
            return await _catCacheRepository.GetCachedCatsByBreedAsync(breedId, 10);
        }
    }

    #endregion

    #region Breed Caching

    public async Task<List<Breed>> GetBreedsAsync(bool forceRefresh = false)
    {
        try
        {
            // Check if cache is valid
            var isCacheValid = !forceRefresh && await _breedCacheRepository.IsBeedCacheValidAsync();
            
            if (isCacheValid)
            {
                var cachedBreeds = await _breedCacheRepository.GetCachedBreedsAsync();
                if (cachedBreeds.Any())
                {
                    return cachedBreeds;
                }
            }

            // Try to get fresh data if online
            if (Connectivity.NetworkAccess == NetworkAccess.Internet)
            {
                var freshBreeds = await _catService.GetBreeds();
                if (freshBreeds?.Any() == true)
                {
                    await _breedCacheRepository.CacheBreedsAsync(freshBreeds);
                    return freshBreeds;
                }
            }

            // Fall back to cached data even if potentially stale
            return await _breedCacheRepository.GetCachedBreedsAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in GetBreedsAsync: {ex.Message}");
            return await _breedCacheRepository.GetCachedBreedsAsync();
        }
    }

    public async Task<Breed> GetBreedByIdAsync(string breedId)
    {
        try
        {
            return await _breedCacheRepository.GetBreedByIdAsync(breedId);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in GetBreedByIdAsync: {ex.Message}");
            return null;
        }
    }

    public async Task<List<Breed>> SearchBreedsAsync(string searchTerm)
    {
        try
        {
            return await _breedCacheRepository.SearchBreedsAsync(searchTerm);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in SearchBreedsAsync: {ex.Message}");
            return new List<Breed>();
        }
    }

    #endregion

    #region Favorites Management

    public async Task<List<FavoriteCatResponse>> GetFavoritesAsync()
    {
        try
        {
            // Always try to sync first if online
            if (Connectivity.NetworkAccess == NetworkAccess.Internet)
            {
                await SyncFavoritesAsync();
            }

            return await _favoriteRepository.GetUserFavoritesAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in GetFavoritesAsync: {ex.Message}");
            return await _favoriteRepository.GetUserFavoritesAsync();
        }
    }

    public async Task<bool> AddFavoriteAsync(Cat cat)
    {
        try
        {
            var success = await _favoriteRepository.AddFavoriteAsync(cat);
            
            // Try to sync immediately if online
            if (success && Connectivity.NetworkAccess == NetworkAccess.Internet)
            {
                _ = Task.Run(() => SyncFavoritesAsync());
            }

            return success;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in AddFavoriteAsync: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> RemoveFavoriteAsync(string imageId)
    {
        try
        {
            var success = await _favoriteRepository.RemoveFavoriteAsync(imageId);
            
            // Try to sync immediately if online
            if (success && Connectivity.NetworkAccess == NetworkAccess.Internet)
            {
                _ = Task.Run(() => SyncFavoritesAsync());
            }

            return success;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in RemoveFavoriteAsync: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> IsFavoriteAsync(string imageId)
    {
        try
        {
            return await _favoriteRepository.IsFavoriteAsync(imageId);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in IsFavoriteAsync: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> SyncFavoritesAsync()
    {
        try
        {
            if (Connectivity.NetworkAccess != NetworkAccess.Internet)
                return false;

            return await _favoriteRepository.SyncFavoritesAsync(_catService);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in SyncFavoritesAsync: {ex.Message}");
            return false;
        }
    }

    public async Task<int> GetUnsyncedFavoritesCountAsync()
    {
        try
        {
            return await _favoriteRepository.GetUnsyncedCountAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in GetUnsyncedFavoritesCountAsync: {ex.Message}");
            return 0;
        }
    }

    #endregion

    #region Cache Management

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
            System.Diagnostics.Debug.WriteLine($"Error in ClearAllCacheAsync: {ex.Message}");
        }
    }

    public async Task PerformCacheMaintenanceAsync()
    {
        try
        {
            // Clean up expired entries
            await _catCacheRepository.CleanupExpiredCacheAsync();
            await _breedCacheRepository.CleanupExpiredCacheAsync();
            
            // Sync favorites if online
            if (Connectivity.NetworkAccess == NetworkAccess.Internet)
            {
                await SyncFavoritesAsync();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in PerformCacheMaintenanceAsync: {ex.Message}");
        }
    }

    public async Task<CacheStatistics> GetCacheStatisticsAsync()
    {
        try
        {
            var stats = new CacheStatistics
            {
                CachedBreedsCount = await _breedCacheRepository.GetBreedCountAsync(),
                FavoritesCount = (await _favoriteRepository.GetUserFavoritesAsync()).Count,
                UnsyncedFavoritesCount = await _favoriteRepository.GetUnsyncedCountAsync(),
                LastCacheUpdate = DateTime.UtcNow,
                IsOnline = Connectivity.NetworkAccess == NetworkAccess.Internet
            };

            return stats;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in GetCacheStatisticsAsync: {ex.Message}");
            return new CacheStatistics
            {
                IsOnline = Connectivity.NetworkAccess == NetworkAccess.Internet
            };
        }
    }

    #endregion

    #region IDisposable

    public async ValueTask DisposeAsync()
    {
        await _catCacheRepository.DisposeAsync();
        await _breedCacheRepository.DisposeAsync();
        await _favoriteRepository.DisposeAsync();
    }

    #endregion
}
