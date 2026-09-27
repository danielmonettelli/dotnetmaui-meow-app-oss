namespace Meow.Infrastructure.Persistence.Entities;

/// <summary>
/// SQLite entity for user favorite cats with sync tracking
/// </summary>
[Table("UserFavorites")]
public class UserFavoriteEntity
{
    [PrimaryKey, AutoIncrement]
    public int LocalId { get; set; }
    public string ImageId { get; set; } = string.Empty;
    public string? FavoriteId { get; set; }
    public string? ImageUrl { get; set; }
    public DateTime AddedAt { get; set; }
    public bool IsSynced { get; set; }
    public bool IsPendingDeletion { get; set; }
    public DateTime? LastSyncAttempt { get; set; }
    public string? BreedsJson { get; set; }

    [Ignore]
    public List<Breed> Breeds
    {
        get => string.IsNullOrEmpty(BreedsJson)
            ? new List<Breed>()
            : JsonSerializer.Deserialize<List<Breed>>(BreedsJson) ?? new List<Breed>();
        set => BreedsJson = JsonSerializer.Serialize(value);
    }

    public FavoriteCatResponse ToFavoriteCatResponse() => new()
    {
        Id = FavoriteId,
        Image = new Cat
        {
            Id = ImageId,
            Url = ImageUrl ?? string.Empty,
            Breeds = Breeds
        },
        Created_at = AddedAt.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
    };

    public static UserFavoriteEntity FromCat(Cat cat) => new()
    {
        ImageId = cat.Id,
        ImageUrl = cat.Url,
        Breeds = cat.Breeds ?? new List<Breed>(),
        AddedAt = DateTime.UtcNow,
        IsSynced = false,
        IsPendingDeletion = false
    };

    public static UserFavoriteEntity FromFavoriteCatResponse(FavoriteCatResponse response) => new()
    {
        FavoriteId = response.Id,
        ImageId = response.Image?.Id ?? string.Empty,
        ImageUrl = response.Image?.Url,
        Breeds = response.Image?.Breeds ?? new List<Breed>(),
        AddedAt = DateTime.TryParse(response.Created_at, out var date) ? date : DateTime.UtcNow,
        IsSynced = true,
        IsPendingDeletion = false,
        LastSyncAttempt = DateTime.UtcNow
    };
}
