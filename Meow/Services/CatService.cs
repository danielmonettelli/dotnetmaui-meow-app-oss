namespace Meow.Services;

public class CatService : ICatService
{
    private readonly HttpClient _httpClient;

    public CatService()
    {
        _httpClient = new HttpClient();
    }

    public async Task<List<Cat>> GetRandomKitty()
    {
        Uri url = new(APIConstants.CustomRandomKittyEndPoint);

        HttpResponseMessage response = await _httpClient.GetAsync(url);

        if (response.IsSuccessStatusCode)
        {
            string content = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<List<Cat>>(content);
        }

        return default;
    }
}
