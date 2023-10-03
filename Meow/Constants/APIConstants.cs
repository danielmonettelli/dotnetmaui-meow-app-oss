namespace Meow.Constants;

public static class APIConstants
{
    public const string APIBaseUrl = "https://api.thecatapi.com/v1/";
    public const string APIKey = "live_HzF8UyYCwcjqY05axorP4F5UFp20fewj8xV5aNX8RTpPkd078qsX7umR5Cqy85qw";

    public const string CustomRandomKittyEndPoint = "images/search?size=med&mime_types=jpg,png&has_breeds=true&include_breeds=true&include_categories=true";
    public const string CustomRandomKittiesByBreedEndPoints = "images/search?size=med&mime_types=jpg,png&limit=10&has_breeds=true&include_breeds=true&include_categories=true";
    public const string BreedsEndPoint = "breeds";
    public const string FavoriteKittensEndPoint = "favourites";
}
