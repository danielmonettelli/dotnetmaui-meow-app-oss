namespace Meow.Core.UseCases;

/// <summary>
/// Use case for managing favorite cats with offline support and sync
/// </summary>
public class ManageFavoritesUseCase
{
    private readonly ICatApiService _catApiService;
    private readonly IFavoriteRepository _favoriteRepository;
    private readonly IConnectivityProvider _connectivity;

    public ManageFavoritesUseCase(
        ICatApiService catApiService,
        IFavoriteRepository favoriteRepository,
        IConnectivityProvider connectivity)
    {
        _catApiService = catApiService;
        _favoriteRepository = favoriteRepository;
        _connectivity = connectivity;
    }

    /// <summary>
    /// Gets all favorites, syncing with server if online
    /// </summary>
    public async Task<Result<List<FavoriteCatResponse>>> GetFavoritesAsync()
    {
        try
        {
            if (_connectivity.IsConnected)
            {
                await SyncAsync();
            }

            var favorites = await _favoriteRepository.GetUserFavoritesAsync();
            return _connectivity.IsConnected
                ? Result<List<FavoriteCatResponse>>.Success(favorites)
                : Result<List<FavoriteCatResponse>>.Offline(favorites);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in GetFavoritesAsync: {ex.Message}");
            var cached = await _favoriteRepository.GetUserFavoritesAsync();
            return Result<List<FavoriteCatResponse>>.Failure(ex.Message, cached, ResultSource.Cache);
        }
    }

    /// <summary>
    /// Adds a cat to favorites (works offline, syncs when back online)
    /// </summary>
    public async Task<Result<bool>> AddFavoriteAsync(Cat cat)
    {
        try
        {
            var success = await _favoriteRepository.AddFavoriteAsync(cat);

            if (!success)
                return Result<bool>.Failure("Cat is already in favorites.");

            // Fire-and-forget sync if online
            if (_connectivity.IsConnected)
            {
                _ = Task.Run(() => SyncAsync());
            }

            return Result<bool>.Success(true,
                _connectivity.IsConnected ? ResultSource.Api : ResultSource.Cache);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in AddFavoriteAsync: {ex.Message}");
            return Result<bool>.Failure($"Failed to add favorite: {ex.Message}");
        }
    }

    /// <summary>
    /// Removes a cat from favorites (marks for sync if synced)
    /// </summary>
    public async Task<Result<bool>> RemoveFavoriteAsync(string imageId)
    {
        try
        {
            var success = await _favoriteRepository.RemoveFavoriteAsync(imageId);

            if (!success)
                return Result<bool>.Failure("Favorite not found.");

            if (_connectivity.IsConnected)
            {
                _ = Task.Run(() => SyncAsync());
            }

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in RemoveFavoriteAsync: {ex.Message}");
            return Result<bool>.Failure($"Failed to remove favorite: {ex.Message}");
        }
    }

    /// <summary>
    /// Checks if a cat is in favorites
    /// </summary>
    public async Task<bool> IsFavoriteAsync(string imageId)
    {
        try
        {
            return await _favoriteRepository.IsFavoriteAsync(imageId);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Syncs favorites with server - bidirectional
    /// </summary>
    public async Task<Result<bool>> SyncAsync()
    {
        if (!_connectivity.IsConnected)
            return Result<bool>.Failure("No internet connection for sync.");

        try
        {
            var success = await _favoriteRepository.SyncFavoritesAsync(_catApiService);
            return success
                ? Result<bool>.Success(true)
                : Result<bool>.Failure("Sync completed with errors.");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in SyncAsync: {ex.Message}");
            return Result<bool>.Failure($"Sync failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets count of favorites pending sync
    /// </summary>
    public async Task<int> GetUnsyncedCountAsync()
    {
        try
        {
            return await _favoriteRepository.GetUnsyncedCountAsync();
        }
        catch
        {
            return 0;
        }
    }
}
