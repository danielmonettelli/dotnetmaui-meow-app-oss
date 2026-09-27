namespace Meow.Tests.Core.UseCases;

/// <summary>
/// Unit tests for GetCatsByBreedUseCase
/// </summary>
public class GetCatsByBreedUseCaseTests
{
    private readonly Mock<ICatApiService> _mockApi;
    private readonly Mock<ICatCacheRepository> _mockCache;
    private readonly Mock<IConnectivityProvider> _mockConnectivity;
    private readonly GetCatsByBreedUseCase _sut;

    public GetCatsByBreedUseCaseTests()
    {
        _mockApi = new Mock<ICatApiService>();
        _mockCache = new Mock<ICatCacheRepository>();
        _mockConnectivity = new Mock<IConnectivityProvider>();

        _sut = new GetCatsByBreedUseCase(
            _mockApi.Object,
            _mockCache.Object,
            _mockConnectivity.Object);
    }

    #region Input Validation

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task ExecuteAsync_WithNullOrEmptyBreedId_ShouldReturnErrorMessage(string? breedId)
    {
        var result = await _sut.ExecuteAsync(breedId!);

        result.ErrorMessage.Should().NotBeNull();
        result.ErrorMessage.Should().Contain("Breed ID is required");
        result.Data.Should().BeEmpty();
    }

    #endregion

    #region Cache Hit

    [Fact]
    public async Task ExecuteAsync_WhenCacheHasData_ShouldReturnCachedCats()
    {
        // Arrange
        var cached = new List<Cat>
        {
            new Cat { Id = "cat1", Url = "https://example.com/cat1.jpg" },
            new Cat { Id = "cat2", Url = "https://example.com/cat2.jpg" }
        };
        _mockCache.Setup(c => c.GetCachedCatsByBreedAsync("abys", 10)).ReturnsAsync(cached);

        // Act
        var result = await _sut.ExecuteAsync("abys");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Source.Should().Be(ResultSource.Cache);
        result.Data.Should().HaveCount(2);
        _mockApi.Verify(a => a.GetCatsByBreedAsync(It.IsAny<string>()), Times.Never);
    }

    #endregion

    #region Force Refresh

    [Fact]
    public async Task ExecuteAsync_WhenForceRefresh_ShouldFetchFromApi()
    {
        // Arrange
        _mockConnectivity.Setup(c => c.IsConnected).Returns(true);
        var cached = new List<Cat> { new Cat { Id = "cached1" } };
        _mockCache.Setup(c => c.GetCachedCatsByBreedAsync("abys", 10)).ReturnsAsync(cached);
        var fresh = new List<Cat> { new Cat { Id = "fresh1" }, new Cat { Id = "fresh2" } };
        _mockApi.Setup(a => a.GetCatsByBreedAsync("abys")).ReturnsAsync(fresh);

        // Act
        var result = await _sut.ExecuteAsync("abys", forceRefresh: true);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Source.Should().Be(ResultSource.Api);
        result.Data.Should().HaveCount(2);
        _mockCache.Verify(c => c.CacheCatsByBreedAsync(fresh, "abys"), Times.Once);
    }

    #endregion

    #region Online - No Cache

    [Fact]
    public async Task ExecuteAsync_WhenNoCacheAndOnline_ShouldFetchAndCache()
    {
        // Arrange
        _mockConnectivity.Setup(c => c.IsConnected).Returns(true);
        _mockCache.Setup(c => c.GetCachedCatsByBreedAsync("beng", 10)).ReturnsAsync(new List<Cat>());
        var fresh = new List<Cat> { new Cat { Id = "bengal1" } };
        _mockApi.Setup(a => a.GetCatsByBreedAsync("beng")).ReturnsAsync(fresh);

        // Act
        var result = await _sut.ExecuteAsync("beng");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Source.Should().Be(ResultSource.Api);
        _mockCache.Verify(c => c.CacheCatsByBreedAsync(fresh, "beng"), Times.Once);
    }

    #endregion

    #region Offline

    [Fact]
    public async Task ExecuteAsync_WhenOfflineWithCache_ShouldReturnCachedOfflineResult()
    {
        // Arrange
        _mockConnectivity.Setup(c => c.IsConnected).Returns(false);
        var cached = new List<Cat> { new Cat { Id = "cached1" } };
        _mockCache.Setup(c => c.GetCachedCatsByBreedAsync("abys", 10)).ReturnsAsync(cached);

        // Act — force refresh would need API, but we're offline, so should fallback
        var result = await _sut.ExecuteAsync("abys", forceRefresh: true);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Source.Should().Be(ResultSource.Cache);
    }

    [Fact]
    public async Task ExecuteAsync_WhenOfflineWithNoCache_ShouldReturnErrorMessage()
    {
        // Arrange
        _mockConnectivity.Setup(c => c.IsConnected).Returns(false);
        _mockCache.Setup(c => c.GetCachedCatsByBreedAsync("abys", 10)).ReturnsAsync(new List<Cat>());

        // Act
        var result = await _sut.ExecuteAsync("abys", forceRefresh: true);

        // Assert
        result.ErrorMessage.Should().NotBeNull();
        result.ErrorMessage.Should().Contain("No cats available");
    }

    #endregion

    #region Exception Handling

    [Fact]
    public async Task ExecuteAsync_WhenHttpRequestException_ShouldFallbackToCache()
    {
        // Arrange
        _mockConnectivity.Setup(c => c.IsConnected).Returns(true);
        // Use SetupSequence so first call returns empty (enters API path), second call returns cached data
        _mockCache.SetupSequence(c => c.GetCachedCatsByBreedAsync("abys", 10))
            .ReturnsAsync(new List<Cat>())  // first call in try block — no cache, goes to API
            .ReturnsAsync(new List<Cat> { new Cat { Id = "cached1" } }); // second call in catch block
        _mockApi.Setup(a => a.GetCatsByBreedAsync("abys")).ThrowsAsync(new HttpRequestException());

        // Act
        var result = await _sut.ExecuteAsync("abys");

        // Assert
        result.ErrorMessage.Should().NotBeNull();
        result.ErrorMessage.Should().Contain("Network error");
        result.Source.Should().Be(ResultSource.Cache);
    }

    #endregion
}
