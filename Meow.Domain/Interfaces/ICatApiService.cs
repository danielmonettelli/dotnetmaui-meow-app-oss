namespace Meow.Domain.Interfaces;

/// <summary>
/// Interface for Cat API service operations
/// </summary>
public interface ICatApiService
{
    /// <summary>
    /// Gets random cats from the API
    /// </summary>
    Task<List<Cat>?> GetRandomCatAsync();

    /// <summary>
    /// Gets random cats from the API with a specified limit
    /// </summary>
    Task<List<Cat>?> GetRandomCatAsync(int limit);

    /// <summary>
    /// Gets all available cat breeds
    /// </summary>
    Task<List<Breed>?> GetBreedsAsync();

    /// <summary>
    /// Gets random cats filtered by a specific breed
    /// </summary>
    Task<List<Cat>?> GetCatsByBreedAsync(string breedId);

    /// <summary>
    /// Gets all favorite cats for the specified user or current installation
    /// </summary>
    Task<List<FavoriteCatResponse>?> GetFavoritesAsync(string? subId = null);

    /// <summary>
    /// Adds a cat to favorites for the specified user or current installation
    /// </summary>
    Task<string?> AddFavoriteAsync(string imageId, string? subId = null);

    /// <summary>
    /// Deletes a cat from favorites using the favorite ID
    /// </summary>
    Task<string?> DeleteFavoriteAsync(int favouriteId);

    /// <summary>
    /// Removes a cat from favorites using the image ID and optional subId
    /// </summary>
    Task<string?> RemoveFavoriteByImageIdAsync(string imageId, string? subId = null);
}
