namespace Meow.Constants;

public static class APIConstants
{
    public const string APIBaseUrl = "https://api.thecatapi.com/v1/";
    public const string APIKey = "live_QiqyXlG2Nrr0wJO927vM0mHsQLeKo3BbNdMjFB3lRCkiti3mA96WSPEEeRPViipd";

    public const string CustomRandomKittyEndPoint = "images/search?size=med&mime_types=jpg,png&has_breeds=true&include_breeds=true&include_categories=true";
    public const string CustomRandomKittiesByBreedEndPoints = "images/search?size=med&mime_types=jpg,png&limit=10&has_breeds=true&include_breeds=true&include_categories=true";
    public const string BreedsEndPoint = "breeds";
    public const string FavoriteKittensEndPoint = "favourites";
}
