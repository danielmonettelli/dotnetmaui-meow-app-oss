namespace Meow.Tests.Core.UseCases;

/// <summary>
/// Unit tests for GetVotingCatsUseCase
/// </summary>
public class GetVotingCatsUseCaseTests
{
    private readonly Mock<ICatApiService> _mockApi;
    private readonly Mock<ICatCacheRepository> _mockCache;
    private readonly Mock<IConnectivityProvider> _mockConnectivity;
    private readonly GetVotingCatsUseCase _sut;

    public GetVotingCatsUseCaseTests()
    {
        _mockApi = new Mock<ICatApiService>();
        _mockCache = new Mock<ICatCacheRepository>();
        _mockConnectivity = new Mock<IConnectivityProvider>();

        _sut = new GetVotingCatsUseCase(
            _mockApi.Object,
            _mockCache.Object,
            _mockConnectivity.Object);
    }

    #region Online - Fresh Data

    [Fact]
    public async Task ExecuteAsync_WhenOnlineAndNeedsRefresh_ShouldFetchFromApi()
    {
        // Arrange
        _mockConnectivity.Setup(c => c.IsConnected).Returns(true);
        _mockCache.Setup(c => c.ShouldRefreshVotingCacheAsync()).ReturnsAsync(true);

        var freshCats = new List<Cat> { new Cat { Id = "cat1" }, new Cat { Id = "cat2" } };
        _mockApi.Setup(a => a.GetRandomCatAsync()).ReturnsAsync(freshCats);
        _mockCache.Setup(c => c.GetCachedVotingCatsAsync(10)).ReturnsAsync(new List<Cat>());

        // Act
        var result = await _sut.ExecuteAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Source.Should().Be(ResultSource.Api);
        result.Data.Should().NotBeNullOrEmpty();
        _mockApi.Verify(a => a.GetRandomCatAsync(), Times.Once);
        _mockCache.Verify(c => c.CacheVotingCatsAsync(freshCats), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenForceRefresh_ShouldBypassCacheCheck()
    {
        // Arrange
        _mockConnectivity.Setup(c => c.IsConnected).Returns(true);
        var freshCats = new List<Cat> { new Cat { Id = "cat1" } };
        _mockApi.Setup(a => a.GetRandomCatAsync()).ReturnsAsync(freshCats);
        _mockCache.Setup(c => c.GetCachedVotingCatsAsync(10)).ReturnsAsync(new List<Cat>());

        // Act
        var result = await _sut.ExecuteAsync(forceRefresh: true);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Source.Should().Be(ResultSource.Api);
        _mockCache.Verify(c => c.ShouldRefreshVotingCacheAsync(), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenOnlineAndCacheFresh_ShouldUseCachedData()
    {
        // Arrange
        _mockConnectivity.Setup(c => c.IsConnected).Returns(true);
        _mockCache.Setup(c => c.ShouldRefreshVotingCacheAsync()).ReturnsAsync(false);
        var cached = new List<Cat> { new Cat { Id = "cached1" } };
        _mockCache.Setup(c => c.GetCachedVotingCatsAsync(15)).ReturnsAsync(cached);

        // Act
        var result = await _sut.ExecuteAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Source.Should().Be(ResultSource.Cache);
        result.Data.Should().HaveCount(1);
        _mockApi.Verify(a => a.GetRandomCatAsync(), Times.Never);
    }

    #endregion

    #region Online - API Failure Fallback

    [Fact]
    public async Task ExecuteAsync_WhenApiReturnsNull_ShouldFallbackToCache()
    {
        // Arrange
        _mockConnectivity.Setup(c => c.IsConnected).Returns(true);
        _mockCache.Setup(c => c.ShouldRefreshVotingCacheAsync()).ReturnsAsync(true);
        _mockApi.Setup(a => a.GetRandomCatAsync()).ReturnsAsync((List<Cat>?)null);
        var cached = new List<Cat> { new Cat { Id = "cached1" } };
        _mockCache.Setup(c => c.GetCachedVotingCatsAsync(15)).ReturnsAsync(cached);

        // Act
        var result = await _sut.ExecuteAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Source.Should().Be(ResultSource.Cache);
    }

    [Fact]
    public async Task ExecuteAsync_WhenApiReturnsEmpty_ShouldFallbackToCache()
    {
        // Arrange
        _mockConnectivity.Setup(c => c.IsConnected).Returns(true);
        _mockCache.Setup(c => c.ShouldRefreshVotingCacheAsync()).ReturnsAsync(true);
        _mockApi.Setup(a => a.GetRandomCatAsync()).ReturnsAsync(new List<Cat>());
        var cached = new List<Cat> { new Cat { Id = "cached1" } };
        _mockCache.Setup(c => c.GetCachedVotingCatsAsync(15)).ReturnsAsync(cached);

        // Act
        var result = await _sut.ExecuteAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    #endregion

    #region Offline

    [Fact]
    public async Task ExecuteAsync_WhenOfflineWithCachedData_ShouldReturnOfflineResult()
    {
        // Arrange
        _mockConnectivity.Setup(c => c.IsConnected).Returns(false);
        _mockCache.Setup(c => c.ShouldRefreshVotingCacheAsync()).ReturnsAsync(true);
        var cached = new List<Cat> { new Cat { Id = "cached1" } };
        _mockCache.Setup(c => c.GetCachedVotingCatsAsync(15)).ReturnsAsync(cached);

        // Act
        var result = await _sut.ExecuteAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Source.Should().Be(ResultSource.Cache);
        result.ErrorMessage.Should().Contain("No internet");
        _mockApi.Verify(a => a.GetRandomCatAsync(), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenOfflineWithNoCache_ShouldReturnOfflineEmptyResult()
    {
        // Arrange
        _mockConnectivity.Setup(c => c.IsConnected).Returns(false);
        _mockCache.Setup(c => c.ShouldRefreshVotingCacheAsync()).ReturnsAsync(true);
        _mockCache.Setup(c => c.GetCachedVotingCatsAsync(15)).ReturnsAsync(new List<Cat>());

        // Act
        var result = await _sut.ExecuteAsync();

        // Assert
        result.Data.Should().BeEmpty();
        result.Source.Should().Be(ResultSource.Cache);
    }

    #endregion

    #region Exception Handling

    [Fact]
    public async Task ExecuteAsync_WhenHttpRequestExceptionThrown_ShouldReturnCachedWithError()
    {
        // Arrange
        _mockConnectivity.Setup(c => c.IsConnected).Returns(true);
        _mockCache.Setup(c => c.ShouldRefreshVotingCacheAsync()).ReturnsAsync(true);
        _mockApi.Setup(a => a.GetRandomCatAsync()).ThrowsAsync(new HttpRequestException("Network error"));
        var cached = new List<Cat> { new Cat { Id = "cached1" } };
        _mockCache.Setup(c => c.GetCachedVotingCatsAsync(10)).ReturnsAsync(cached);

        // Act
        var result = await _sut.ExecuteAsync();

        // Assert
        result.ErrorMessage.Should().Contain("Network error");
        result.Source.Should().Be(ResultSource.Cache);
        result.Data.Should().HaveCount(1);
    }

    [Fact]
    public async Task ExecuteAsync_WhenTaskCanceledException_ShouldReturnCachedWithTimeout()
    {
        // Arrange
        _mockConnectivity.Setup(c => c.IsConnected).Returns(true);
        _mockCache.Setup(c => c.ShouldRefreshVotingCacheAsync()).ReturnsAsync(true);
        _mockApi.Setup(a => a.GetRandomCatAsync()).ThrowsAsync(new TaskCanceledException());
        var cached = new List<Cat> { new Cat { Id = "cached1" } };
        _mockCache.Setup(c => c.GetCachedVotingCatsAsync(10)).ReturnsAsync(cached);

        // Act
        var result = await _sut.ExecuteAsync();

        // Assert
        result.ErrorMessage.Should().Contain("timed out");
        result.Source.Should().Be(ResultSource.Cache);
    }

    [Fact]
    public async Task ExecuteAsync_WhenGenericException_ShouldReturnCachedWithError()
    {
        // Arrange
        _mockConnectivity.Setup(c => c.IsConnected).Returns(true);
        _mockCache.Setup(c => c.ShouldRefreshVotingCacheAsync()).ReturnsAsync(true);
        _mockApi.Setup(a => a.GetRandomCatAsync()).ThrowsAsync(new InvalidOperationException("Unexpected"));
        var cached = new List<Cat> { new Cat { Id = "cached1" } };
        _mockCache.Setup(c => c.GetCachedVotingCatsAsync(10)).ReturnsAsync(cached);

        // Act
        var result = await _sut.ExecuteAsync();

        // Assert
        result.ErrorMessage.Should().Contain("Unexpected");
        result.Source.Should().Be(ResultSource.Cache);
    }

    #endregion

    #region Deduplication

    [Fact]
    public async Task ExecuteAsync_ShouldDeduplicateFreshAndCachedCats()
    {
        // Arrange
        _mockConnectivity.Setup(c => c.IsConnected).Returns(true);
        _mockCache.Setup(c => c.ShouldRefreshVotingCacheAsync()).ReturnsAsync(true);

        var freshCats = new List<Cat> { new Cat { Id = "cat1" }, new Cat { Id = "cat2" } };
        var cachedCats = new List<Cat> { new Cat { Id = "cat1" }, new Cat { Id = "cat3" } };

        _mockApi.Setup(a => a.GetRandomCatAsync()).ReturnsAsync(freshCats);
        _mockCache.Setup(c => c.GetCachedVotingCatsAsync(10)).ReturnsAsync(cachedCats);

        // Act
        var result = await _sut.ExecuteAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        // cat1 appears in both, should only appear once
        result.Data!.Select(c => c.Id).Should().OnlyHaveUniqueItems();
    }

    #endregion
}
