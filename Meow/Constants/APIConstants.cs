namespace Meow.Constants;

public static class APIConstants
{
    public const string APIBaseUrl = "https://api.thecatapi.com/v1";
    public const string APIKey = "THECAT_API_KEY_HERE";

    public const string CustomRandomKittyEndPoint = $"{APIBaseUrl}/images/search?size=med&mime_types=jpg,png&has_breeds=true&include_breeds=true&include_categories=true";
}
