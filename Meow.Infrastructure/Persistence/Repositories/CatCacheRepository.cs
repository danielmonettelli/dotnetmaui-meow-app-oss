namespace Meow.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository for cached cat data implementing ICatCacheRepository
/// </summary>
public class CatCacheRepository : BaseRepository, ICatCacheRepository
{
    public CatCacheRepository(IDatabasePathProvider pathProvider) : base(pathProvider) { }

    public async Task<List<Cat>> GetCachedVotingCatsAsync(int limit = 10)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var database = await GetDatabaseAsync();
            var cached = await database.Table<CachedCatEntity>()
                .Where(c => c.CacheType == CacheTypes.Voting)
                .OrderByDescending(c => c.LastAccessed)
                .Take(limit)
                .ToListAsync();

            foreach (var cat in cached)
            {
                cat.LastAccessed = DateTime.UtcNow;
                await database.UpdateAsync(cat);
            }

            return cached.Select(c => c.ToCat()).ToList();
        }, new List<Cat>());
    }

    public async Task CacheVotingCatsAsync(List<Cat> cats)
    {
        await ExecuteSafelyAsync(async () =>
        {
            var database = await GetDatabaseAsync();

            foreach (var cat in cats)
            {
                var existing = await database.Table<CachedCatEntity>()
                    .Where(c => c.Id == cat.Id && c.CacheType == CacheTypes.Voting)
                    .FirstOrDefaultAsync();

                if (existing == null)
                {
                    await database.InsertAsync(CachedCatEntity.FromCat(cat, CacheTypes.Voting));
                }
                else
                {
                    existing.LastAccessed = DateTime.UtcNow;
                    await database.UpdateAsync(existing);
                }
            }

            await MaintainCacheSizeAsync(database);
            return true;
        }, false);
    }

    public async Task<List<Cat>> GetCachedCatsByBreedAsync(string breedId, int limit = 10)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var database = await GetDatabaseAsync();
            var cached = await database.Table<CachedCatEntity>()
                .Where(c => c.CacheType == CacheTypes.Breed && c.BreedId == breedId)
                .OrderByDescending(c => c.CachedAt)
                .Take(limit)
                .ToListAsync();

            return cached.Select(c => c.ToCat()).ToList();
        }, new List<Cat>());
    }

    public async Task CacheCatsByBreedAsync(List<Cat> cats, string breedId)
    {
        await ExecuteSafelyAsync(async () =>
        {
            var database = await GetDatabaseAsync();
            await database.ExecuteAsync(
                "DELETE FROM CachedCats WHERE CacheType = ? AND BreedId = ?",
                CacheTypes.Breed, breedId);

            foreach (var cat in cats)
            {
                await database.InsertAsync(CachedCatEntity.FromCat(cat, CacheTypes.Breed, breedId));
            }
            return true;
        }, false);
    }

    public async Task<bool> ShouldRefreshVotingCacheAsync()
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var database = await GetDatabaseAsync();
            var count = await database.Table<CachedCatEntity>()
                .Where(c => c.CacheType == CacheTypes.Voting)
                .CountAsync();
            return count < 10;
        }, true);
    }

    public async Task ClearCacheAsync()
    {
        await ExecuteSafelyAsync(async () =>
        {
            var database = await GetDatabaseAsync();
            await database.DeleteAllAsync<CachedCatEntity>();
            return true;
        }, false);
    }

    public async Task CleanupExpiredCacheAsync()
    {
        await ExecuteSafelyAsync(async () =>
        {
            var database = await GetDatabaseAsync();
            var votingExpiry = DateTime.UtcNow.AddHours(-DatabaseConfig.VotingCacheExpirationHours);
            await database.ExecuteAsync(
                "DELETE FROM CachedCats WHERE CacheType = ? AND CachedAt < ?",
                CacheTypes.Voting, votingExpiry);
            return true;
        }, false);
    }

    private async Task MaintainCacheSizeAsync(SQLiteAsyncConnection database)
    {
        var count = await database.Table<CachedCatEntity>()
            .Where(c => c.CacheType == CacheTypes.Voting)
            .CountAsync();

        if (count > DatabaseConfig.MaxCachedVotingCats)
        {
            var excess = count - DatabaseConfig.MaxCachedVotingCats;
            var oldest = await database.Table<CachedCatEntity>()
                .Where(c => c.CacheType == CacheTypes.Voting)
                .OrderBy(c => c.LastAccessed)
                .Take(excess)
                .ToListAsync();

            foreach (var cat in oldest)
            {
                await database.DeleteAsync(cat);
            }
        }
    }
}
