namespace Meow.Tests.Core.UseCases;

/// <summary>
/// Unit tests for GetBreedsUseCase
/// </summary>
public class GetBreedsUseCaseTests
{
    private readonly Mock<ICatApiService> _mockApi;
    private readonly Mock<IBreedCacheRepository> _mockCache;
    private readonly Mock<IConnectivityProvider> _mockConnectivity;
    private readonly GetBreedsUseCase _sut;

    public GetBreedsUseCaseTests()
    {
        _mockApi = new Mock<ICatApiService>();
        _mockCache = new Mock<IBreedCacheRepository>();
        _mockConnectivity = new Mock<IConnectivityProvider>();

        _sut = new GetBreedsUseCase(
            _mockApi.Object,
            _mockCache.Object,
            _mockConnectivity.Object);
    }

    #region Cache Valid

    [Fact]
    public async Task ExecuteAsync_WhenCacheIsValid_ShouldReturnCachedBreeds()
    {
        // Arrange
        var breeds = new List<Breed> { new Breed { Id = "abys", Name = "Abyssinian" } };
        _mockCache.Setup(c => c.IsBreedCacheValidAsync()).ReturnsAsync(true);
        _mockCache.Setup(c => c.GetCachedBreedsAsync()).ReturnsAsync(breeds);

        // Act
        var result = await _sut.ExecuteAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Source.Should().Be(ResultSource.Cache);
        result.Data.Should().HaveCount(1);
        result.Data!.First().Name.Should().Be("Abyssinian");
        _mockApi.Verify(a => a.GetBreedsAsync(), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCacheValidButEmpty_ShouldFetchFromApi()
    {
        // Arrange
        _mockConnectivity.Setup(c => c.IsConnected).Returns(true);
        _mockCache.Setup(c => c.IsBreedCacheValidAsync()).ReturnsAsync(true);
        _mockCache.Setup(c => c.GetCachedBreedsAsync()).ReturnsAsync(new List<Breed>());

        var apiBreeds = new List<Breed> { new Breed { Id = "beng", Name = "Bengal" } };
        _mockApi.Setup(a => a.GetBreedsAsync()).ReturnsAsync(apiBreeds);

        // Act
        var result = await _sut.ExecuteAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Source.Should().Be(ResultSource.Api);
    }

    #endregion

    #region Force Refresh

    [Fact]
    public async Task ExecuteAsync_WhenForceRefresh_ShouldBypassCache()
    {
        // Arrange
        _mockConnectivity.Setup(c => c.IsConnected).Returns(true);
        var freshBreeds = new List<Breed> { new Breed { Id = "siam", Name = "Siamese" } };
        _mockApi.Setup(a => a.GetBreedsAsync()).ReturnsAsync(freshBreeds);

        // Act
        var result = await _sut.ExecuteAsync(forceRefresh: true);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Source.Should().Be(ResultSource.Api);
        _mockCache.Verify(c => c.IsBreedCacheValidAsync(), Times.Never);
        _mockCache.Verify(c => c.CacheBreedsAsync(freshBreeds), Times.Once);
    }

    #endregion

    #region Online - Fresh Fetch

    [Fact]
    public async Task ExecuteAsync_WhenCacheInvalidAndOnline_ShouldFetchAndCache()
    {
        // Arrange
        _mockConnectivity.Setup(c => c.IsConnected).Returns(true);
        _mockCache.Setup(c => c.IsBreedCacheValidAsync()).ReturnsAsync(false);
        var freshBreeds = new List<Breed> { new Breed { Id = "pers", Name = "Persian" } };
        _mockApi.Setup(a => a.GetBreedsAsync()).ReturnsAsync(freshBreeds);

        // Act
        var result = await _sut.ExecuteAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Source.Should().Be(ResultSource.Api);
        _mockCache.Verify(c => c.CacheBreedsAsync(freshBreeds), Times.Once);
    }

    #endregion

    #region Offline

    [Fact]
    public async Task ExecuteAsync_WhenOfflineWithStaleCache_ShouldReturnOfflineResult()
    {
        // Arrange
        _mockConnectivity.Setup(c => c.IsConnected).Returns(false);
        _mockCache.Setup(c => c.IsBreedCacheValidAsync()).ReturnsAsync(false);
        var stale = new List<Breed> { new Breed { Id = "abys", Name = "Abyssinian" } };
        _mockCache.Setup(c => c.GetCachedBreedsAsync()).ReturnsAsync(stale);

        // Act
        var result = await _sut.ExecuteAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Source.Should().Be(ResultSource.Cache);
        result.ErrorMessage.Should().Contain("No internet");
    }

    [Fact]
    public async Task ExecuteAsync_WhenOfflineWithNoCache_ShouldReturnErrorMessage()
    {
        // Arrange
        _mockConnectivity.Setup(c => c.IsConnected).Returns(false);
        _mockCache.Setup(c => c.IsBreedCacheValidAsync()).ReturnsAsync(false);
        _mockCache.Setup(c => c.GetCachedBreedsAsync()).ReturnsAsync(new List<Breed>());

        // Act
        var result = await _sut.ExecuteAsync();

        // Assert
        result.ErrorMessage.Should().NotBeNull();
        result.ErrorMessage.Should().Contain("No breeds available");
    }

    #endregion

    #region Exception Handling

    [Fact]
    public async Task ExecuteAsync_WhenHttpRequestException_ShouldFallbackToCache()
    {
        // Arrange
        _mockConnectivity.Setup(c => c.IsConnected).Returns(true);
        _mockCache.Setup(c => c.IsBreedCacheValidAsync()).ReturnsAsync(false);
        _mockApi.Setup(a => a.GetBreedsAsync()).ThrowsAsync(new HttpRequestException());
        var cached = new List<Breed> { new Breed { Id = "abys" } };
        _mockCache.Setup(c => c.GetCachedBreedsAsync()).ReturnsAsync(cached);

        // Act
        var result = await _sut.ExecuteAsync();

        // Assert
        result.ErrorMessage.Should().Contain("Network error");
        result.Source.Should().Be(ResultSource.Cache);
        result.Data.Should().HaveCount(1);
    }

    [Fact]
    public async Task ExecuteAsync_WhenTaskCanceledException_ShouldFallbackToCache()
    {
        // Arrange
        _mockConnectivity.Setup(c => c.IsConnected).Returns(true);
        _mockCache.Setup(c => c.IsBreedCacheValidAsync()).ReturnsAsync(false);
        _mockApi.Setup(a => a.GetBreedsAsync()).ThrowsAsync(new TaskCanceledException());
        var cached = new List<Breed> { new Breed { Id = "abys" } };
        _mockCache.Setup(c => c.GetCachedBreedsAsync()).ReturnsAsync(cached);

        // Act
        var result = await _sut.ExecuteAsync();

        // Assert
        result.ErrorMessage.Should().Contain("timed out");
        result.Source.Should().Be(ResultSource.Cache);
    }

    #endregion
}
