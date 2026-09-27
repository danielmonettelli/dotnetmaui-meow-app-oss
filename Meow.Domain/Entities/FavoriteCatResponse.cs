namespace Meow.Domain.Entities;

/// <summary>
/// Response model for a favorite cat from the API
/// </summary>
public class FavoriteCatResponse
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("user_id")]
    public string? User_id { get; set; }

    [JsonPropertyName("image_id")]
    public string? Image_id { get; set; }

    [JsonPropertyName("sub_id")]
    public string? Sub_id { get; set; }

    [JsonPropertyName("created_at")]
    public string? Created_at { get; set; }

    [JsonPropertyName("image")]
    public Cat? Image { get; set; }
}

/// <summary>
/// Image details within a favorite response
/// </summary>
public class Image
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }
}
