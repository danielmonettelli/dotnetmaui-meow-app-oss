namespace Meow.Services;

/// <summary>
/// Service for interacting with the Cat API
/// </summary>
public class CatService : ICatService
{
    #region Private Fields

    private readonly HttpClient _httpClient;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the CatService
    /// </summary>
    public CatService()
    {
        _httpClient = new HttpClient();
        _httpClient.BaseAddress = new Uri(APIConstants.APIBaseUrl);
        _httpClient.DefaultRequestHeaders.Add("x-api-key", APIConstants.APIKey);
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    #endregion

    #region Cat Data Methods

    /// <summary>
    /// Gets a random cat from the API
    /// </summary>
    /// <returns>List containing a random cat</returns>
    public async Task<List<Cat>> GetRandomKitty()
    {
        HttpResponseMessage response = await _httpClient.GetAsync(APIConstants.CustomRandomKittyEndPoint);

        if (response.IsSuccessStatusCode)
        {
            string content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<Cat>>(content);
        }

        return default;
    }

    /// <summary>
    /// Gets all available cat breeds
    /// </summary>
    /// <returns>List of all cat breeds</returns>
    public async Task<List<Breed>> GetBreeds()
    {
        HttpResponseMessage response = await _httpClient.GetAsync(APIConstants.BreedsEndPoint);

        if (response.IsSuccessStatusCode)
        {
            string content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<Breed>>(content);
        }

        return default;
    }

    /// <summary>
    /// Gets random kittens filtered by a specific breed
    /// </summary>
    /// <param name="breed">Breed ID to filter by</param>
    /// <returns>List of cats of the specified breed</returns>
    public async Task<List<Cat>> GetRandomKittensByBreed(string breed)
    {
        HttpResponseMessage response = await _httpClient.GetAsync($"{APIConstants.CustomRandomKittiesByBreedEndPoints}&breed_ids={breed}");

        if (response.IsSuccessStatusCode)
        {
            string content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<Cat>>(content);
        }

        return default;
    }

    #endregion

    #region Favorites Management Methods

    /// <summary>
    /// Gets all favorite cats for the current user
    /// </summary>
    /// <returns>List of favorite cat responses</returns>
    public async Task<List<FavoriteCatResponse>> GetFavoriteKittens()
    {
        HttpResponseMessage response = await _httpClient.GetAsync(APIConstants.FavoriteKittensEndPoint);

        if (response.IsSuccessStatusCode)
        {
            string content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<FavoriteCatResponse>>(content);
        }

        return default;
    }

    /// <summary>
    /// Adds a cat to the user's favorites
    /// </summary>
    /// <param name="cat_id">ID of the cat image to add</param>
    /// <returns>Response from the API</returns>
    public async Task<string> AddFavoriteKitten(string cat_id)
    {
        // Create request object
        var favoriteCatRequest = new FavoriteCatRequest()
        {
            Image_id = cat_id
        };

        // Serialize the request
        var body = JsonSerializer.Serialize(favoriteCatRequest);
        var content = new StringContent(body, Encoding.UTF8, "application/json");

        // Send POST request
        var response = await _httpClient.PostAsync(APIConstants.FavoriteKittensEndPoint, content);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadAsStringAsync();
        }

        return default;
    }

    /// <summary>
    /// Deletes a cat from favorites using the favorite ID
    /// </summary>
    /// <param name="favourite_id">ID of the favorite entry</param>
    /// <returns>Response from the API</returns>
    public async Task<string> DeleteFavoriteKitten(int favourite_id)
    {
        var response = await _httpClient.DeleteAsync($"{APIConstants.FavoriteKittensEndPoint}/{favourite_id}");

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadAsStringAsync();
        }

        return default;
    }

    /// <summary>
    /// Removes a cat from favorites using the cat ID
    /// </summary>
    /// <param name="cat_id">ID of the cat to remove</param>
    /// <returns>Response from the API</returns>
    public async Task<string> RemoveFavoriteKitten(string cat_id)
    {
        // Find the favorite cat response with the matching Image_id
        var favoriteCatResponse = (await GetFavoriteKittens()).FirstOrDefault(x => x.Image_id == cat_id);

        if (favoriteCatResponse != null)
        {
            var response = await _httpClient.DeleteAsync($"{APIConstants.FavoriteKittensEndPoint}/{favoriteCatResponse.Id}");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }
        }

        return default;
    }

    #endregion
}
