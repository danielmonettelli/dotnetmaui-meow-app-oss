using System.Net.Http.Headers;

namespace Meow.Infrastructure.Api;

/// <summary>
/// Implementation of ICatApiService using HttpClient
/// </summary>
public class CatApiService : ICatApiService
{
    private readonly HttpClient _httpClient;

    public CatApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<Cat>?> GetRandomCatAsync() => await GetRandomCatAsync(15);

    public async Task<List<Cat>?> GetRandomCatAsync(int limit)
    {
        try
        {
            var nonce = $"{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}_{Random.Shared.Next(1000, 9999)}";
            var endpoint = $"{ApiConstants.RandomCatEndpoint}&limit={limit}&order=RANDOM&_={nonce}";
            using var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
            request.Headers.CacheControl = new CacheControlHeaderValue { NoCache = true, NoStore = true };
            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<Cat>>(content);
            }
            return null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"GetRandomCatAsync error: {ex.Message}");
            return null;
        }
    }

    public async Task<List<Breed>?> GetBreedsAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync(ApiConstants.BreedsEndpoint);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<Breed>>(content);
            }
            return null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"GetBreedsAsync error: {ex.Message}");
            return null;
        }
    }

    public async Task<List<Cat>?> GetCatsByBreedAsync(string breedId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{ApiConstants.CatsByBreedEndpoint}&breed_ids={breedId}");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<Cat>>(content);
            }
            return null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"GetCatsByBreedAsync error: {ex.Message}");
            return null;
        }
    }

    public async Task<List<FavoriteCatResponse>?> GetFavoritesAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync(ApiConstants.FavoritesEndpoint);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<FavoriteCatResponse>>(content);
            }
            return null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"GetFavoritesAsync error: {ex.Message}");
            return null;
        }
    }

    public async Task<string?> AddFavoriteAsync(string imageId)
    {
        try
        {
            var request = new FavoriteCatRequest { Image_id = imageId };
            var body = JsonSerializer.Serialize(request);
            var content = new StringContent(body, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(ApiConstants.FavoritesEndpoint, content);

            return response.IsSuccessStatusCode
                ? await response.Content.ReadAsStringAsync()
                : null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"AddFavoriteAsync error: {ex.Message}");
            return null;
        }
    }

    public async Task<string?> DeleteFavoriteAsync(int favouriteId)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"{ApiConstants.FavoritesEndpoint}/{favouriteId}");
            return response.IsSuccessStatusCode
                ? await response.Content.ReadAsStringAsync()
                : null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"DeleteFavoriteAsync error: {ex.Message}");
            return null;
        }
    }

    public async Task<string?> RemoveFavoriteByImageIdAsync(string imageId)
    {
        try
        {
            var favorites = await GetFavoritesAsync();
            var target = favorites?.FirstOrDefault(x => x.Image_id == imageId);

            if (target?.Id != null && int.TryParse(target.Id, out var favouriteId))
            {
                return await DeleteFavoriteAsync(favouriteId);
            }
            return null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"RemoveFavoriteByImageIdAsync error: {ex.Message}");
            return null;
        }
    }
}
