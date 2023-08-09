namespace Meow.Models;

public class FavoriteCatResponse
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("user_id")]
    public string User_id { get; set; }

    [JsonPropertyName("image_id")]
    public string Image_id { get; set; }

    [JsonPropertyName("sub_id")]
    public string Sub_id { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime Created_at { get; set; }

    [JsonPropertyName("image")]
    public Image ImageCat { get; set; }
}

public class Image
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("url")]
    public string Url { get; set; }
}
