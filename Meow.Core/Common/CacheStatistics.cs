namespace Meow.Core.Common;

/// <summary>
/// Cache statistics for monitoring cache health
/// </summary>
public class CacheStatistics
{
    public int CachedCatsCount { get; set; }
    public int CachedBreedsCount { get; set; }
    public int FavoritesCount { get; set; }
    public int UnsyncedFavoritesCount { get; set; }
    public DateTime LastCacheUpdate { get; set; }
    public bool IsOnline { get; set; }
}
