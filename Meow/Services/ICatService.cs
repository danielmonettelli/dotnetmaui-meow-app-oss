namespace Meow.Services;

/// <summary>
/// Service interface for interacting with the Cat API
/// </summary>
public interface ICatService
{
    #region Cat Data Methods

    /// <summary>
    /// Gets a random cat from the API
    /// </summary>
    /// <returns>List containing a random cat</returns>
    Task<List<Cat>> GetRandomKitty();

    /// <summary>
    /// Gets all available cat breeds
    /// </summary>
    /// <returns>List of all cat breeds</returns>
    Task<List<Breed>> GetBreeds();

    /// <summary>
    /// Gets random kittens filtered by a specific breed
    /// </summary>
    /// <param name="breed">Breed ID to filter by</param>
    /// <returns>List of cats of the specified breed</returns>
    Task<List<Cat>> GetRandomKittensByBreed(string breed);

    #endregion

    #region Favorites Management Methods

    /// <summary>
    /// Gets all favorite cats for the current user
    /// </summary>
    /// <returns>List of favorite cat responses</returns>
    Task<List<FavoriteCatResponse>> GetFavoriteKittens();

    /// <summary>
    /// Adds a cat to the user's favorites
    /// </summary>
    /// <param name="image_id">ID of the cat image to add</param>
    /// <returns>Response from the API</returns>
    Task<string> AddFavoriteKitten(string image_id);

    /// <summary>
    /// Deletes a cat from favorites using the favorite ID
    /// </summary>
    /// <param name="favourite_id">ID of the favorite entry</param>
    /// <returns>Response from the API</returns>
    Task<string> DeleteFavoriteKitten(int favourite_id);

    /// <summary>
    /// Removes a cat from favorites using the cat ID
    /// </summary>
    /// <param name="cat_id">ID of the cat to remove</param>
    /// <returns>Response from the API</returns>
    Task<string> RemoveFavoriteKitten(string cat_id);

    #endregion
}
