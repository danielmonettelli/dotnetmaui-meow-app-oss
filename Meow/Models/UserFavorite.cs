namespace Meow.Models;

/// <summary>
/// User favorite cats for local persistence
/// </summary>
[Table("UserFavorites")]
public class UserFavorite
{
    [PrimaryKey, AutoIncrement]
    public int LocalId { get; set; }

    /// <summary>
    /// Cat image ID
    /// </summary>
    public string ImageId { get; set; }

    /// <summary>
    /// API favorite ID (from TheCatAPI)
    /// </summary>
    public string FavoriteId { get; set; }

    /// <summary>
    /// Cat image URL
    /// </summary>
    public string ImageUrl { get; set; }

    /// <summary>
    /// When the favorite was added locally
    /// </summary>
    public DateTime AddedAt { get; set; }

    /// <summary>
    /// Whether this favorite is synced with the server
    /// </summary>
    public bool IsSynced { get; set; }

    /// <summary>
    /// Whether this favorite is pending deletion
    /// </summary>
    public bool IsPendingDeletion { get; set; }

    /// <summary>
    /// Last sync attempt timestamp
    /// </summary>
    public DateTime? LastSyncAttempt { get; set; }

    /// <summary>
    /// Serialized JSON of cat breed information
    /// </summary>
    public string BreedsJson { get; set; }

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
    /// Converts to FavoriteCatResponse model
    /// </summary>
    public FavoriteCatResponse ToFavoriteCatResponse()
    {
        return new FavoriteCatResponse
        {
            Id = this.FavoriteId,
            Image = new Cat
            {
                Id = this.ImageId,
                Url = this.ImageUrl,
                Breeds = this.Breeds
            },
            Created_at = this.AddedAt.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
        };
    }

    /// <summary>
    /// Creates from FavoriteCatResponse model
    /// </summary>
    public static UserFavorite FromFavoriteCatResponse(FavoriteCatResponse response)
    {
        return new UserFavorite
        {
            FavoriteId = response.Id,
            ImageId = response.Image?.Id,
            ImageUrl = response.Image?.Url,
            Breeds = response.Image?.Breeds ?? new List<Breed>(),
            AddedAt = DateTime.TryParse(response.Created_at, out var date) ? date : DateTime.UtcNow,
            IsSynced = true,
            IsPendingDeletion = false,
            LastSyncAttempt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates from Cat model for local favorites
    /// </summary>
    public static UserFavorite FromCat(Cat cat)
    {
        return new UserFavorite
        {
            ImageId = cat.Id,
            ImageUrl = cat.Url,
            Breeds = cat.Breeds ?? new List<Breed>(),
            AddedAt = DateTime.UtcNow,
            IsSynced = false,
            IsPendingDeletion = false
        };
    }
}
