namespace Meow.Infrastructure.Persistence.Entities;

/// <summary>
/// SQLite entity for cached cat data
/// </summary>
[Table("CachedCats")]
public class CachedCatEntity
{
    [PrimaryKey]
    public string Id { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public int Width { get; set; }
    public int Height { get; set; }
    public string? BreedsJson { get; set; }
    public string CacheType { get; set; } = string.Empty;
    public string? BreedId { get; set; }
    public DateTime CachedAt { get; set; }
    public DateTime LastAccessed { get; set; }

    [Ignore]
    public List<Breed> Breeds
    {
        get => string.IsNullOrEmpty(BreedsJson)
            ? new List<Breed>()
            : JsonSerializer.Deserialize<List<Breed>>(BreedsJson) ?? new List<Breed>();
        set => BreedsJson = JsonSerializer.Serialize(value);
    }

    public Cat ToCat() => new()
    {
        Id = Id,
        Url = Url,
        Width = Width,
        Height = Height,
        Breeds = Breeds
    };

    public static CachedCatEntity FromCat(Cat cat, string cacheType, string? breedId = null) => new()
    {
        Id = cat.Id,
        Url = cat.Url,
        Width = cat.Width,
        Height = cat.Height,
        Breeds = cat.Breeds ?? new List<Breed>(),
        CacheType = cacheType,
        BreedId = breedId,
        CachedAt = DateTime.UtcNow,
        LastAccessed = DateTime.UtcNow
    };
}

/// <summary>
/// Cache type constants
/// </summary>
public static class CacheTypes
{
    public const string Voting = "Voting";
    public const string Breed = "Breed";
    public const string Favorite = "Favorite";
}
