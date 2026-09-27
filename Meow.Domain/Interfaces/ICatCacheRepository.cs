namespace Meow.Domain.Interfaces;

/// <summary>
/// Interface for cat cache repository operations
/// </summary>
public interface ICatCacheRepository
{
    Task<List<Cat>> GetCachedVotingCatsAsync(int limit = 10);
    Task CacheVotingCatsAsync(List<Cat> cats);
    Task<List<Cat>> GetCachedCatsByBreedAsync(string breedId, int limit = 10);
    Task CacheCatsByBreedAsync(List<Cat> cats, string breedId);
    Task<bool> ShouldRefreshVotingCacheAsync();
    Task ClearCacheAsync();
    Task CleanupExpiredCacheAsync();
    ValueTask DisposeAsync();
}
