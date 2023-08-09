namespace Meow.Models;

public class FavoriteCatRequest
{
    [JsonPropertyName("image_id")]
    public string Image_id { get; set; }

    //[JsonPropertyName("sub_id")]
    //public string Sub_id { get; set; }
}
