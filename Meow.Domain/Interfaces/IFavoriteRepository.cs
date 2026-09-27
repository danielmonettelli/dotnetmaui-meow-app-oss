namespace Meow.Domain.Interfaces;

/// <summary>
/// Interface for favorites repository operations
/// </summary>
public interface IFavoriteRepository
{
    Task<List<FavoriteCatResponse>> GetUserFavoritesAsync();
    Task<bool> AddFavoriteAsync(Cat cat);
    Task<bool> RemoveFavoriteAsync(string imageId);
    Task<bool> IsFavoriteAsync(string imageId);
    Task<bool> SyncFavoritesAsync(ICatApiService catApiService);
    Task<int> GetUnsyncedCountAsync();
    Task ClearFavoritesAsync();
    ValueTask DisposeAsync();
}
