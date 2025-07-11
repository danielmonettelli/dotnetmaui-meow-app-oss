namespace Meow.Data;

/// <summary>
/// Repository for managing cached cat data for voting
/// </summary>
public class CatCacheRepository : BaseRepository
{
    /// <summary>
    /// Gets cached cats for voting, prioritizing most recently accessed
    /// </summary>
    public async Task<List<Cat>> GetCachedVotingCatsAsync(int limit = 10)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var database = await GetDatabaseAsync();
            
            var cachedCats = await database.Table<CachedCat>()
                .Where(c => c.CacheType == CacheTypes.Voting)
                .OrderByDescending(c => c.LastAccessed)
                .Take(limit)
                .ToListAsync();

            // Update last accessed time
            foreach (var cat in cachedCats)
            {
                cat.LastAccessed = DateTime.UtcNow;
                await database.UpdateAsync(cat);
            }

            return cachedCats.Select(c => c.ToCat()).ToList();
        }, new List<Cat>());
    }

    /// <summary>
    /// Caches new cats for voting, maintaining the maximum limit
    /// </summary>
    public async Task CacheVotingCatsAsync(List<Cat> cats)
    {
        await ExecuteSafelyAsync(async () =>
        {
            var database = await GetDatabaseAsync();

            // Add new cats to cache
            foreach (var cat in cats)
            {
                var existingCat = await database.Table<CachedCat>()
                    .Where(c => c.Id == cat.Id && c.CacheType == CacheTypes.Voting)
                    .FirstOrDefaultAsync();

                if (existingCat == null)
                {
                    var cachedCat = CachedCat.FromCat(cat, CacheTypes.Voting);
                    await database.InsertAsync(cachedCat);
                }
                else
                {
                    // Update existing entry
                    existingCat.LastAccessed = DateTime.UtcNow;
                    await database.UpdateAsync(existingCat);
                }
            }

            // Maintain cache size limit
            await MaintainVotingCacheSizeAsync();
            
            return true;
        }, false);
    }

    /// <summary>
    /// Gets cached cats for a specific breed
    /// </summary>
    public async Task<List<Cat>> GetCachedCatsByBreedAsync(string breedId, int limit = 10)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var database = await GetDatabaseAsync();
            
            var cachedCats = await database.Table<CachedCat>()
                .Where(c => c.CacheType == CacheTypes.Breed && c.BreedId == breedId)
                .OrderByDescending(c => c.CachedAt)
                .Take(limit)
                .ToListAsync();

            return cachedCats.Select(c => c.ToCat()).ToList();
        }, new List<Cat>());
    }

    /// <summary>
    /// Caches cats for a specific breed
    /// </summary>
    public async Task CacheCatsByBreedAsync(List<Cat> cats, string breedId)
    {
        await ExecuteSafelyAsync(async () =>
        {
            var database = await GetDatabaseAsync();

            // Clear existing breed cache
            await database.ExecuteAsync(
                "DELETE FROM CachedCats WHERE CacheType = ? AND BreedId = ?", 
                CacheTypes.Breed, breedId);

            // Add new cats
            foreach (var cat in cats)
            {
                var cachedCat = CachedCat.FromCat(cat, CacheTypes.Breed, breedId);
                await database.InsertAsync(cachedCat);
            }

            return true;
        }, false);
    }

    /// <summary>
    /// Checks if voting cache needs refresh
    /// </summary>
    public async Task<bool> ShouldRefreshVotingCacheAsync()
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var database = await GetDatabaseAsync();
            
            var cacheCount = await database.Table<CachedCat>()
                .Where(c => c.CacheType == CacheTypes.Voting)
                .CountAsync();

            // Refresh if we have fewer than 10 cached cats or if we have internet
            return cacheCount < 10 || IsConnected;
        }, true);
    }

    /// <summary>
    /// Maintains the voting cache size within limits
    /// </summary>
    private async Task MaintainVotingCacheSizeAsync()
    {
        var database = await GetDatabaseAsync();
        
        var cacheCount = await database.Table<CachedCat>()
            .Where(c => c.CacheType == CacheTypes.Voting)
            .CountAsync();

        if (cacheCount > DatabaseConstants.MaxCachedVotingCats)
        {
            // Get oldest entries to remove
            var excessCount = cacheCount - DatabaseConstants.MaxCachedVotingCats;
            var oldestCats = await database.Table<CachedCat>()
                .Where(c => c.CacheType == CacheTypes.Voting)
                .OrderBy(c => c.LastAccessed)
                .Take(excessCount)
                .ToListAsync();

            foreach (var cat in oldestCats)
            {
                await database.DeleteAsync(cat);
            }
        }
    }

    /// <summary>
    /// Clears all cached cats
    /// </summary>
    public async Task ClearCacheAsync()
    {
        await ExecuteSafelyAsync(async () =>
        {
            var database = await GetDatabaseAsync();
            await database.DeleteAllAsync<CachedCat>();
            return true;
        }, false);
    }
}
