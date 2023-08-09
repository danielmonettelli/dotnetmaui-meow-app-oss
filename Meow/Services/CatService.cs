namespace Meow.Services;

public class CatService : ICatService
{
    private readonly HttpClient _httpClient;

    public CatService()
    {
        _httpClient = new HttpClient();

        _httpClient.BaseAddress = new Uri(APIConstants.APIBaseUrl);

        _httpClient.DefaultRequestHeaders.Add("x-api-key", APIConstants.APIKey);

        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

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

    public async Task<string> AddFavoriteKitten(string cat_id)
    {
        var favoriteCatRequest = new FavoriteCatRequest()
        {
            Image_id = cat_id
        };

        var body = JsonSerializer.Serialize(favoriteCatRequest);

        var content = new StringContent(body, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(APIConstants.FavoriteKittensEndPoint, content);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadAsStringAsync();
        }

        return default;
    }

    public async Task<string> DeleteFavoriteKitten(int favourite_id)
    {
        var response = await _httpClient.DeleteAsync($"{APIConstants.FavoriteKittensEndPoint}/{favourite_id}");

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadAsStringAsync();
        }

        return default;
    }
}
