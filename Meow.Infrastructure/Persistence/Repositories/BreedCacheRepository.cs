namespace Meow.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository for cached breed data implementing IBreedCacheRepository
/// </summary>
public class BreedCacheRepository : BaseRepository, IBreedCacheRepository
{
    public BreedCacheRepository(IDatabasePathProvider pathProvider) : base(pathProvider) { }

    public async Task<List<Breed>> GetCachedBreedsAsync()
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var database = await GetDatabaseAsync();
            var cached = await database.Table<CachedBreedEntity>()
                .OrderBy(b => b.Name)
                .ToListAsync();
            return cached.Select(b => b.ToBreed()).ToList();
        }, new List<Breed>());
    }

    public async Task CacheBreedsAsync(List<Breed> breeds)
    {
        await ExecuteSafelyAsync(async () =>
        {
            var database = await GetDatabaseAsync();
            await database.DeleteAllAsync<CachedBreedEntity>();

            foreach (var breed in breeds)
            {
                await database.InsertAsync(CachedBreedEntity.FromBreed(breed));
            }
            return true;
        }, false);
    }

    public async Task<Breed?> GetBreedByIdAsync(string breedId)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var database = await GetDatabaseAsync();
            var cached = await database.Table<CachedBreedEntity>()
                .Where(b => b.Id == breedId)
                .FirstOrDefaultAsync();
            return cached?.ToBreed();
        });
    }

    public async Task<bool> IsBreedCacheValidAsync()
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var database = await GetDatabaseAsync();
            var count = await database.Table<CachedBreedEntity>().CountAsync();

            if (count == 0) return false;

            var oldest = await database.Table<CachedBreedEntity>()
                .OrderBy(b => b.CachedAt)
                .FirstOrDefaultAsync();

            if (oldest == null) return false;

            return (DateTime.UtcNow - oldest.CachedAt).TotalDays < DatabaseConfig.BreedCacheExpirationDays;
        }, false);
    }

    public async Task<List<Breed>> SearchBreedsAsync(string searchTerm)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var database = await GetDatabaseAsync();
            var term = searchTerm.ToLower();
            var cached = await database.Table<CachedBreedEntity>()
                .Where(b => b.Name.ToLower().Contains(term) ||
                           b.Temperament.ToLower().Contains(term) ||
                           b.Origin.ToLower().Contains(term))
                .OrderBy(b => b.Name)
                .ToListAsync();
            return cached.Select(b => b.ToBreed()).ToList();
        }, new List<Breed>());
    }

    public async Task<int> GetBreedCountAsync()
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var database = await GetDatabaseAsync();
            return await database.Table<CachedBreedEntity>().CountAsync();
        }, 0);
    }

    public async Task ClearCacheAsync()
    {
        await ExecuteSafelyAsync(async () =>
        {
            var database = await GetDatabaseAsync();
            await database.DeleteAllAsync<CachedBreedEntity>();
            return true;
        }, false);
    }

    public async Task CleanupExpiredCacheAsync()
    {
        await ExecuteSafelyAsync(async () =>
        {
            var database = await GetDatabaseAsync();
            var breedExpiry = DateTime.UtcNow.AddDays(-DatabaseConfig.BreedCacheExpirationDays);
            await database.ExecuteAsync("DELETE FROM CachedBreeds WHERE CachedAt < ?", breedExpiry);
            return true;
        }, false);
    }
}
