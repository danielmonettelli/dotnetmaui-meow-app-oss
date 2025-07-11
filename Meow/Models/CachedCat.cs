namespace Meow.Models;

/// <summary>
/// Cached cat entity for local storage
/// </summary>
[Table("CachedCats")]
public class CachedCat
{
    [PrimaryKey]
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("url")]
    public string Url { get; set; }

    [JsonPropertyName("width")]
    public int Width { get; set; }

    [JsonPropertyName("height")]
    public int Height { get; set; }

    /// <summary>
    /// Serialized JSON of breed information
    /// </summary>
    public string BreedsJson { get; set; }

    /// <summary>
    /// Cache type: Voting, Breed, or Favorite
    /// </summary>
    public string CacheType { get; set; }

    /// <summary>
    /// Breed ID for breed-specific cats
    /// </summary>
    public string BreedId { get; set; }

    /// <summary>
    /// When this item was cached
    /// </summary>
    public DateTime CachedAt { get; set; }

    /// <summary>
    /// Last time this item was accessed
    /// </summary>
    public DateTime LastAccessed { get; set; }

    /// <summary>
    /// Deserialized breeds collection
    /// </summary>
    [Ignore]
    public List<Breed> Breeds
    {
        get => string.IsNullOrEmpty(BreedsJson) 
            ? new List<Breed>() 
            : JsonSerializer.Deserialize<List<Breed>>(BreedsJson);
        set => BreedsJson = JsonSerializer.Serialize(value);
    }

    /// <summary>
    /// Converts to Cat model
    /// </summary>
    public Cat ToCat()
    {
        return new Cat
        {
            Id = this.Id,
            Url = this.Url,
            Width = this.Width,
            Height = this.Height,
            Breeds = this.Breeds
        };
    }

    /// <summary>
    /// Creates from Cat model
    /// </summary>
    public static CachedCat FromCat(Cat cat, string cacheType, string breedId = null)
    {
        return new CachedCat
        {
            Id = cat.Id,
            Url = cat.Url,
            Width = cat.Width,
            Height = cat.Height,
            Breeds = cat.Breeds,
            CacheType = cacheType,
            BreedId = breedId,
            CachedAt = DateTime.UtcNow,
            LastAccessed = DateTime.UtcNow
        };
    }
}

/// <summary>
/// Cache types for categorizing cached data
/// </summary>
public static class CacheTypes
{
    public const string Voting = "Voting";
    public const string Breed = "Breed";
    public const string Favorite = "Favorite";
}
