namespace Meow.Data;

/// <summary>
/// Repository for managing cached breed data
/// </summary>
public class BreedCacheRepository : BaseRepository
{
    /// <summary>
    /// Gets all cached breeds
    /// </summary>
    public async Task<List<Breed>> GetCachedBreedsAsync()
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var database = await GetDatabaseAsync();
            
            var cachedBreeds = await database.Table<CachedBreed>()
                .OrderBy(b => b.Name)
                .ToListAsync();

            return cachedBreeds.Select(b => b.ToBreed()).ToList();
        }, new List<Breed>());
    }

    /// <summary>
    /// Caches breed data
    /// </summary>
    public async Task CacheBreedsAsync(List<Breed> breeds)
    {
        await ExecuteSafelyAsync(async () =>
        {
            var database = await GetDatabaseAsync();

            // Clear existing breed cache
            await database.DeleteAllAsync<CachedBreed>();

            // Add new breeds
            foreach (var breed in breeds)
            {
                var cachedBreed = CachedBreed.FromBreed(breed);
                await database.InsertAsync(cachedBreed);
            }

            return true;
        }, false);
    }

    /// <summary>
    /// Gets a specific breed by ID
    /// </summary>
    public async Task<Breed> GetBreedByIdAsync(string breedId)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var database = await GetDatabaseAsync();
            
            var cachedBreed = await database.Table<CachedBreed>()
                .Where(b => b.Id == breedId)
                .FirstOrDefaultAsync();

            return cachedBreed?.ToBreed();
        });
    }

    /// <summary>
    /// Checks if breed cache exists and is valid
    /// </summary>
    public async Task<bool> IsBeedCacheValidAsync()
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var database = await GetDatabaseAsync();
            
            var breedCount = await database.Table<CachedBreed>().CountAsync();
            
            if (breedCount == 0)
                return false;

            // Check if cache is not too old
            var oldestBreed = await database.Table<CachedBreed>()
                .OrderBy(b => b.CachedAt)
                .FirstOrDefaultAsync();

            if (oldestBreed == null)
                return false;

            var cacheAge = DateTime.UtcNow - oldestBreed.CachedAt;
            return cacheAge.TotalDays < DatabaseConstants.BreedCacheExpirationDays;
        }, false);
    }

    /// <summary>
    /// Searches breeds by name or characteristics
    /// </summary>
    public async Task<List<Breed>> SearchBreedsAsync(string searchTerm)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var database = await GetDatabaseAsync();
            
            var searchTermLower = searchTerm.ToLower();
            
            var cachedBreeds = await database.Table<CachedBreed>()
                .Where(b => b.Name.ToLower().Contains(searchTermLower) || 
                           b.Temperament.ToLower().Contains(searchTermLower) ||
                           b.Origin.ToLower().Contains(searchTermLower))
                .OrderBy(b => b.Name)
                .ToListAsync();

            return cachedBreeds.Select(b => b.ToBreed()).ToList();
        }, new List<Breed>());
    }

    /// <summary>
    /// Gets breed count
    /// </summary>
    public async Task<int> GetBreedCountAsync()
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var database = await GetDatabaseAsync();
            return await database.Table<CachedBreed>().CountAsync();
        }, 0);
    }

    /// <summary>
    /// Clears all cached breeds
    /// </summary>
    public async Task ClearCacheAsync()
    {
        await ExecuteSafelyAsync(async () =>
        {
            var database = await GetDatabaseAsync();
            await database.DeleteAllAsync<CachedBreed>();
            return true;
        }, false);
    }
}
