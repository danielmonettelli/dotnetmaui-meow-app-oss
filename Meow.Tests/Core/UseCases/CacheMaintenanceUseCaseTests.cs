namespace Meow.Tests.Core.UseCases;

/// <summary>
/// Unit tests for CacheMaintenanceUseCase
/// </summary>
public class CacheMaintenanceUseCaseTests
{
    private readonly Mock<ICatCacheRepository> _mockCatCache;
    private readonly Mock<IBreedCacheRepository> _mockBreedCache;
    private readonly Mock<IFavoriteRepository> _mockFavoriteRepo;
    private readonly Mock<IConnectivityProvider> _mockConnectivity;
    private readonly Mock<ICatApiService> _mockApi;
    private readonly CacheMaintenanceUseCase _sut;

    public CacheMaintenanceUseCaseTests()
    {
        _mockCatCache = new Mock<ICatCacheRepository>();
        _mockBreedCache = new Mock<IBreedCacheRepository>();
        _mockFavoriteRepo = new Mock<IFavoriteRepository>();
        _mockConnectivity = new Mock<IConnectivityProvider>();
        _mockApi = new Mock<ICatApiService>();

        _sut = new CacheMaintenanceUseCase(
            _mockCatCache.Object,
            _mockBreedCache.Object,
            _mockFavoriteRepo.Object,
            _mockConnectivity.Object,
            _mockApi.Object);
    }

    #region ExecuteMaintenanceAsync

    [Fact]
    public async Task ExecuteMaintenanceAsync_ShouldCleanupExpiredCaches()
    {
        // Arrange
        _mockConnectivity.Setup(c => c.IsConnected).Returns(false);

        // Act
        await _sut.ExecuteMaintenanceAsync();

        // Assert
        _mockCatCache.Verify(c => c.CleanupExpiredCacheAsync(), Times.Once);
        _mockBreedCache.Verify(c => c.CleanupExpiredCacheAsync(), Times.Once);
    }

    [Fact]
    public async Task ExecuteMaintenanceAsync_WhenOnline_ShouldSyncFavorites()
    {
        // Arrange
        _mockConnectivity.Setup(c => c.IsConnected).Returns(true);
        _mockFavoriteRepo.Setup(r => r.SyncFavoritesAsync(_mockApi.Object)).ReturnsAsync(true);

        // Act
        await _sut.ExecuteMaintenanceAsync();

        // Assert
        _mockFavoriteRepo.Verify(r => r.SyncFavoritesAsync(_mockApi.Object), Times.Once);
    }

    [Fact]
    public async Task ExecuteMaintenanceAsync_WhenOffline_ShouldNotSyncFavorites()
    {
        // Arrange
        _mockConnectivity.Setup(c => c.IsConnected).Returns(false);

        // Act
        await _sut.ExecuteMaintenanceAsync();

        // Assert
        _mockFavoriteRepo.Verify(r => r.SyncFavoritesAsync(It.IsAny<ICatApiService>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteMaintenanceAsync_WhenExceptionThrown_ShouldNotThrow()
    {
        // Arrange
        _mockCatCache.Setup(c => c.CleanupExpiredCacheAsync())
            .ThrowsAsync(new Exception("Cleanup failed"));

        // Act & Assert — should not throw
        await _sut.Invoking(s => s.ExecuteMaintenanceAsync())
            .Should().NotThrowAsync();
    }

    #endregion

    #region ClearAllCacheAsync

    [Fact]
    public async Task ClearAllCacheAsync_ShouldClearAllRepositories()
    {
        // Act
        await _sut.ClearAllCacheAsync();

        // Assert
        _mockCatCache.Verify(c => c.ClearCacheAsync(), Times.Once);
        _mockBreedCache.Verify(c => c.ClearCacheAsync(), Times.Once);
        _mockFavoriteRepo.Verify(r => r.ClearFavoritesAsync(), Times.Once);
    }

    [Fact]
    public async Task ClearAllCacheAsync_WhenExceptionThrown_ShouldNotThrow()
    {
        // Arrange
        _mockCatCache.Setup(c => c.ClearCacheAsync())
            .ThrowsAsync(new Exception("Clear failed"));

        // Act & Assert
        await _sut.Invoking(s => s.ClearAllCacheAsync())
            .Should().NotThrowAsync();
    }

    #endregion

    #region GetStatisticsAsync

    [Fact]
    public async Task GetStatisticsAsync_ShouldReturnCacheStatistics()
    {
        // Arrange
        _mockBreedCache.Setup(c => c.GetBreedCountAsync()).ReturnsAsync(67);
        _mockFavoriteRepo.Setup(r => r.GetUserFavoritesAsync())
            .ReturnsAsync(new List<FavoriteCatResponse>
            {
                new FavoriteCatResponse { Id = "1" },
                new FavoriteCatResponse { Id = "2" }
            });
        _mockFavoriteRepo.Setup(r => r.GetUnsyncedCountAsync()).ReturnsAsync(1);
        _mockConnectivity.Setup(c => c.IsConnected).Returns(true);

        // Act
        var stats = await _sut.GetStatisticsAsync();

        // Assert
        stats.CachedBreedsCount.Should().Be(67);
        stats.FavoritesCount.Should().Be(2);
        stats.UnsyncedFavoritesCount.Should().Be(1);
        stats.IsOnline.Should().BeTrue();
    }

    [Fact]
    public async Task GetStatisticsAsync_WhenOffline_ShouldReportOffline()
    {
        // Arrange
        _mockBreedCache.Setup(c => c.GetBreedCountAsync()).ReturnsAsync(0);
        _mockFavoriteRepo.Setup(r => r.GetUserFavoritesAsync())
            .ReturnsAsync(new List<FavoriteCatResponse>());
        _mockFavoriteRepo.Setup(r => r.GetUnsyncedCountAsync()).ReturnsAsync(0);
        _mockConnectivity.Setup(c => c.IsConnected).Returns(false);

        // Act
        var stats = await _sut.GetStatisticsAsync();

        // Assert
        stats.IsOnline.Should().BeFalse();
    }

    [Fact]
    public async Task GetStatisticsAsync_WhenException_ShouldReturnDefaultWithConnectivity()
    {
        // Arrange
        _mockBreedCache.Setup(c => c.GetBreedCountAsync())
            .ThrowsAsync(new Exception("DB error"));
        _mockConnectivity.Setup(c => c.IsConnected).Returns(true);

        // Act
        var stats = await _sut.GetStatisticsAsync();

        // Assert
        stats.IsOnline.Should().BeTrue();
        stats.CachedBreedsCount.Should().Be(0);
    }

    #endregion
}
