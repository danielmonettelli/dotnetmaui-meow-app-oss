namespace Meow.Domain.Entities;

/// <summary>
/// Cat entity representing a cat image from the API
/// </summary>
public class Cat
{
    [JsonPropertyName("breeds")]
    public List<Breed>? Breeds { get; set; }

    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    [JsonPropertyName("width")]
    public int Width { get; set; }

    [JsonPropertyName("height")]
    public int Height { get; set; }
}
