namespace Meow.Core.UseCases;

/// <summary>
/// Use case for getting cats by breed with caching and offline support
/// </summary>
public class GetCatsByBreedUseCase
{
    private readonly ICatApiService _catApiService;
    private readonly ICatCacheRepository _catCacheRepository;
    private readonly IConnectivityProvider _connectivity;

    public GetCatsByBreedUseCase(
        ICatApiService catApiService,
        ICatCacheRepository catCacheRepository,
        IConnectivityProvider connectivity)
    {
        _catApiService = catApiService;
        _catCacheRepository = catCacheRepository;
        _connectivity = connectivity;
    }

    public async Task<Result<List<Cat>>> ExecuteAsync(string breedId, bool forceRefresh = false)
    {
        if (string.IsNullOrEmpty(breedId))
            return Result<List<Cat>>.Failure("Breed ID is required.", new List<Cat>());

        try
        {
            // Check cache first unless forcing refresh
            var cachedCats = await _catCacheRepository.GetCachedCatsByBreedAsync(breedId, 10);

            if (!forceRefresh && cachedCats.Count > 0)
            {
                return Result<List<Cat>>.FromCache(cachedCats);
            }

            if (_connectivity.IsConnected)
            {
                var freshCats = await _catApiService.GetCatsByBreedAsync(breedId);
                if (freshCats?.Count > 0)
                {
                    await _catCacheRepository.CacheCatsByBreedAsync(freshCats, breedId);
                    return Result<List<Cat>>.Success(freshCats);
                }
            }

            return cachedCats.Count > 0
                ? Result<List<Cat>>.Offline(cachedCats)
                : Result<List<Cat>>.Failure("No cats available for this breed.", new List<Cat>());
        }
        catch (HttpRequestException)
        {
            var cached = await _catCacheRepository.GetCachedCatsByBreedAsync(breedId, 10);
            return Result<List<Cat>>.Failure("Network error.", cached, ResultSource.Cache);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in GetCatsByBreedUseCase: {ex.Message}");
            var cached = await _catCacheRepository.GetCachedCatsByBreedAsync(breedId, 10);
            return Result<List<Cat>>.Failure(ex.Message, cached, ResultSource.Cache);
        }
    }
}
