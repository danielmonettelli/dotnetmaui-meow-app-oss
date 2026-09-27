namespace Meow.Domain.Interfaces;

/// <summary>
/// Interface for breed cache repository operations
/// </summary>
public interface IBreedCacheRepository
{
    Task<List<Breed>> GetCachedBreedsAsync();
    Task CacheBreedsAsync(List<Breed> breeds);
    Task<Breed?> GetBreedByIdAsync(string breedId);
    Task<bool> IsBreedCacheValidAsync();
    Task<List<Breed>> SearchBreedsAsync(string searchTerm);
    Task<int> GetBreedCountAsync();
    Task ClearCacheAsync();
    Task CleanupExpiredCacheAsync();
    ValueTask DisposeAsync();
}
