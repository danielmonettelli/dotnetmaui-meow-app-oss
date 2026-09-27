namespace Meow.Core.UseCases;

/// <summary>
/// Use case for getting cat breeds with caching and offline support
/// </summary>
public class GetBreedsUseCase
{
    private readonly ICatApiService _catApiService;
    private readonly IBreedCacheRepository _breedCacheRepository;
    private readonly IConnectivityProvider _connectivity;

    public GetBreedsUseCase(
        ICatApiService catApiService,
        IBreedCacheRepository breedCacheRepository,
        IConnectivityProvider connectivity)
    {
        _catApiService = catApiService;
        _breedCacheRepository = breedCacheRepository;
        _connectivity = connectivity;
    }

    public async Task<Result<List<Breed>>> ExecuteAsync(bool forceRefresh = false)
    {
        try
        {
            var isCacheValid = !forceRefresh && await _breedCacheRepository.IsBreedCacheValidAsync();

            if (isCacheValid)
            {
                var cachedBreeds = await _breedCacheRepository.GetCachedBreedsAsync();
                if (cachedBreeds.Count > 0)
                {
                    return Result<List<Breed>>.FromCache(cachedBreeds);
                }
            }

            if (_connectivity.IsConnected)
            {
                var freshBreeds = await _catApiService.GetBreedsAsync();
                if (freshBreeds?.Count > 0)
                {
                    await _breedCacheRepository.CacheBreedsAsync(freshBreeds);
                    return Result<List<Breed>>.Success(freshBreeds);
                }
            }

            // Fallback to cached data even if stale
            var fallback = await _breedCacheRepository.GetCachedBreedsAsync();
            return fallback.Count > 0
                ? Result<List<Breed>>.Offline(fallback)
                : Result<List<Breed>>.Failure("No breeds available. Please check your connection.", new List<Breed>());
        }
        catch (HttpRequestException)
        {
            var cached = await _breedCacheRepository.GetCachedBreedsAsync();
            return Result<List<Breed>>.Failure("Network error. Showing cached breeds.", cached, ResultSource.Cache);
        }
        catch (TaskCanceledException)
        {
            var cached = await _breedCacheRepository.GetCachedBreedsAsync();
            return Result<List<Breed>>.Failure("Request timed out. Showing cached breeds.", cached, ResultSource.Cache);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in GetBreedsUseCase: {ex.Message}");
            var cached = await _breedCacheRepository.GetCachedBreedsAsync();
            return Result<List<Breed>>.Failure(ex.Message, cached, ResultSource.Cache);
        }
    }
}
