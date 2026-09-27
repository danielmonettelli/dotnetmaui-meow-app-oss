namespace Meow.Core.UseCases;

/// <summary>
/// Use case for getting cats for voting with cache-first strategy and offline support
/// </summary>
public class GetVotingCatsUseCase
{
    private readonly ICatApiService _catApiService;
    private readonly ICatCacheRepository _catCacheRepository;
    private readonly IConnectivityProvider _connectivity;

    public GetVotingCatsUseCase(
        ICatApiService catApiService,
        ICatCacheRepository catCacheRepository,
        IConnectivityProvider connectivity)
    {
        _catApiService = catApiService;
        _catCacheRepository = catCacheRepository;
        _connectivity = connectivity;
    }

    public async Task<Result<List<Cat>>> ExecuteAsync(bool forceRefresh = false)
    {
        try
        {
            var shouldRefresh = forceRefresh || await _catCacheRepository.ShouldRefreshVotingCacheAsync();

            if (shouldRefresh && _connectivity.IsConnected)
            {
                var freshCats = await _catApiService.GetRandomCatAsync();
                if (freshCats?.Count > 0)
                {
                    await _catCacheRepository.CacheVotingCatsAsync(freshCats);
                    return Result<List<Cat>>.Success(freshCats, ResultSource.Api);
                }
            }

            // Offline or API failed — fall back to cache
            var cachedData = await _catCacheRepository.GetCachedVotingCatsAsync(15);

            if (cachedData.Count > 0)
            {
                return _connectivity.IsConnected
                    ? Result<List<Cat>>.FromCache(cachedData)
                    : Result<List<Cat>>.Offline(cachedData);
            }

            if (!_connectivity.IsConnected)
            {
                return Result<List<Cat>>.Offline(new List<Cat>());
            }

            // Last resort: direct API call
            var lastResort = await _catApiService.GetRandomCatAsync();
            return lastResort?.Count > 0
                ? Result<List<Cat>>.Success(lastResort)
                : Result<List<Cat>>.Failure("Unable to load cats.", new List<Cat>());
        }
        catch (HttpRequestException)
        {
            var cached = await _catCacheRepository.GetCachedVotingCatsAsync(10);
            return Result<List<Cat>>.Failure("Network error. Showing cached data.", cached, ResultSource.Cache);
        }
        catch (TaskCanceledException)
        {
            var cached = await _catCacheRepository.GetCachedVotingCatsAsync(10);
            return Result<List<Cat>>.Failure("Request timed out. Showing cached data.", cached, ResultSource.Cache);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in GetVotingCatsUseCase: {ex.Message}");
            var cached = await _catCacheRepository.GetCachedVotingCatsAsync(10);
            return Result<List<Cat>>.Failure(ex.Message, cached, ResultSource.Cache);
        }
    }
}
