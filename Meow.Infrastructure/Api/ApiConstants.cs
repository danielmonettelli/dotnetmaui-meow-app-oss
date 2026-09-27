namespace Meow.Infrastructure.Api;

/// <summary>
/// API constants for TheCatAPI endpoints and configuration
/// </summary>
public static class ApiConstants
{
    public const string BaseUrl = "https://api.thecatapi.com/v1/";
    public const string ApiKey = "YOUR_API_KEY_HERE";

    public const string RandomCatEndpoint = "images/search?size=med&mime_types=jpg,png&include_breeds=true";
    public const string CatsByBreedEndpoint = "images/search?size=med&mime_types=jpg,png&limit=10&has_breeds=true&include_breeds=true&include_categories=true";
    public const string BreedsEndpoint = "breeds";
    public const string FavoritesEndpoint = "favourites";
}
