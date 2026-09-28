namespace Meow.Domain.Entities;

/// <summary>
/// Request model for adding a cat to favorites
/// </summary>
public class FavoriteCatRequest
{
    [JsonPropertyName("image_id")]
    public string Image_id { get; set; } = string.Empty;

    [JsonPropertyName("sub_id")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Sub_id { get; set; }
}
