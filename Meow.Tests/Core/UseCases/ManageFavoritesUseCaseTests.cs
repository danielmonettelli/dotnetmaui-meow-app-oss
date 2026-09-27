namespace Meow.Tests.Core.UseCases;

/// <summary>
/// Unit tests for ManageFavoritesUseCase
/// </summary>
public class ManageFavoritesUseCaseTests
{
    private readonly Mock<ICatApiService> _mockApi;
    private readonly Mock<IFavoriteRepository> _mockRepo;
    private readonly Mock<IConnectivityProvider> _mockConnectivity;
    private readonly ManageFavoritesUseCase _sut;

    public ManageFavoritesUseCaseTests()
    {
        _mockApi = new Mock<ICatApiService>();
        _mockRepo = new Mock<IFavoriteRepository>();
        _mockConnectivity = new Mock<IConnectivityProvider>();

        _sut = new ManageFavoritesUseCase(
            _mockApi.Object,
            _mockRepo.Object,
            _mockConnectivity.Object);
    }

    #region GetFavoritesAsync

    [Fact]
    public async Task GetFavoritesAsync_WhenOnline_ShouldSyncAndReturnFavorites()
    {
        // Arrange
        _mockConnectivity.Setup(c => c.IsConnected).Returns(true);
        _mockRepo.Setup(r => r.SyncFavoritesAsync(_mockApi.Object)).ReturnsAsync(true);
        var favorites = new List<FavoriteCatResponse>
        {
            new FavoriteCatResponse { Id = "fav1", Image = new Cat { Id = "cat1" } }
        };
        _mockRepo.Setup(r => r.GetUserFavoritesAsync()).ReturnsAsync(favorites);

        // Act
        var result = await _sut.GetFavoritesAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Source.Should().Be(ResultSource.Api);
        result.Data.Should().HaveCount(1);
        _mockRepo.Verify(r => r.SyncFavoritesAsync(_mockApi.Object), Times.Once);
    }

    [Fact]
    public async Task GetFavoritesAsync_WhenOffline_ShouldReturnCachedFavorites()
    {
        // Arrange
        _mockConnectivity.Setup(c => c.IsConnected).Returns(false);
        var favorites = new List<FavoriteCatResponse>
        {
            new FavoriteCatResponse { Id = "fav1", Image = new Cat { Id = "cat1" } }
        };
        _mockRepo.Setup(r => r.GetUserFavoritesAsync()).ReturnsAsync(favorites);

        // Act
        var result = await _sut.GetFavoritesAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Source.Should().Be(ResultSource.Cache);
        result.Data.Should().HaveCount(1);
        _mockRepo.Verify(r => r.SyncFavoritesAsync(It.IsAny<ICatApiService>()), Times.Never);
    }

    [Fact]
    public async Task GetFavoritesAsync_WhenSyncThrows_ShouldStillReturnFavorites()
    {
        // Arrange: SyncAsync catches its own exceptions internally, so GetFavoritesAsync
        // will proceed past the sync call and return favorites from the repository.
        _mockConnectivity.Setup(c => c.IsConnected).Returns(true);
        _mockRepo.Setup(r => r.SyncFavoritesAsync(_mockApi.Object))
            .ThrowsAsync(new Exception("Sync failed"));
        var cached = new List<FavoriteCatResponse>();
        _mockRepo.Setup(r => r.GetUserFavoritesAsync()).ReturnsAsync(cached);

        // Act
        var result = await _sut.GetFavoritesAsync();

        // Assert: SyncAsync swallows the exception, so GetFavoritesAsync succeeds
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
    }

    #endregion

    #region AddFavoriteAsync

    [Fact]
    public async Task AddFavoriteAsync_WhenSuccess_ShouldReturnSuccessResult()
    {
        // Arrange
        var cat = new Cat { Id = "cat1", Url = "https://example.com/cat1.jpg" };
        _mockConnectivity.Setup(c => c.IsConnected).Returns(true);
        _mockRepo.Setup(r => r.AddFavoriteAsync(cat)).ReturnsAsync(true);

        // Act
        var result = await _sut.AddFavoriteAsync(cat);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeTrue();
        result.Source.Should().Be(ResultSource.Api);
    }

    [Fact]
    public async Task AddFavoriteAsync_WhenOffline_ShouldReturnCacheSource()
    {
        // Arrange
        var cat = new Cat { Id = "cat1" };
        _mockConnectivity.Setup(c => c.IsConnected).Returns(false);
        _mockRepo.Setup(r => r.AddFavoriteAsync(cat)).ReturnsAsync(true);

        // Act
        var result = await _sut.AddFavoriteAsync(cat);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Source.Should().Be(ResultSource.Cache);
    }

    [Fact]
    public async Task AddFavoriteAsync_WhenAlreadyFavorite_ShouldReturnFailureMessage()
    {
        // Arrange
        var cat = new Cat { Id = "cat1" };
        _mockRepo.Setup(r => r.AddFavoriteAsync(cat)).ReturnsAsync(false);

        // Act
        var result = await _sut.AddFavoriteAsync(cat);

        // Assert
        result.ErrorMessage.Should().NotBeNull();
        result.ErrorMessage.Should().Contain("already in favorites");
    }

    [Fact]
    public async Task AddFavoriteAsync_WhenException_ShouldReturnErrorMessage()
    {
        // Arrange
        var cat = new Cat { Id = "cat1" };
        _mockRepo.Setup(r => r.AddFavoriteAsync(cat))
            .ThrowsAsync(new Exception("DB error"));

        // Act
        var result = await _sut.AddFavoriteAsync(cat);

        // Assert
        result.ErrorMessage.Should().NotBeNull();
        result.ErrorMessage.Should().Contain("Failed to add favorite");
    }

    #endregion

    #region RemoveFavoriteAsync

    [Fact]
    public async Task RemoveFavoriteAsync_WhenSuccess_ShouldReturnSuccessResult()
    {
        // Arrange
        _mockConnectivity.Setup(c => c.IsConnected).Returns(true);
        _mockRepo.Setup(r => r.RemoveFavoriteAsync("cat1")).ReturnsAsync(true);

        // Act
        var result = await _sut.RemoveFavoriteAsync("cat1");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeTrue();
    }

    [Fact]
    public async Task RemoveFavoriteAsync_WhenNotFound_ShouldReturnErrorMessage()
    {
        // Arrange
        _mockRepo.Setup(r => r.RemoveFavoriteAsync("cat1")).ReturnsAsync(false);

        // Act
        var result = await _sut.RemoveFavoriteAsync("cat1");

        // Assert
        result.ErrorMessage.Should().NotBeNull();
        result.ErrorMessage.Should().Contain("Favorite not found");
    }

    [Fact]
    public async Task RemoveFavoriteAsync_WhenException_ShouldReturnErrorMessage()
    {
        // Arrange
        _mockRepo.Setup(r => r.RemoveFavoriteAsync("cat1"))
            .ThrowsAsync(new Exception("DB error"));

        // Act
        var result = await _sut.RemoveFavoriteAsync("cat1");

        // Assert
        result.ErrorMessage.Should().NotBeNull();
        result.ErrorMessage.Should().Contain("Failed to remove favorite");
    }

    #endregion

    #region IsFavoriteAsync

    [Fact]
    public async Task IsFavoriteAsync_WhenFavorite_ShouldReturnTrue()
    {
        _mockRepo.Setup(r => r.IsFavoriteAsync("cat1")).ReturnsAsync(true);

        var result = await _sut.IsFavoriteAsync("cat1");

        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsFavoriteAsync_WhenNotFavorite_ShouldReturnFalse()
    {
        _mockRepo.Setup(r => r.IsFavoriteAsync("cat1")).ReturnsAsync(false);

        var result = await _sut.IsFavoriteAsync("cat1");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsFavoriteAsync_WhenException_ShouldReturnFalse()
    {
        _mockRepo.Setup(r => r.IsFavoriteAsync("cat1"))
            .ThrowsAsync(new Exception("DB error"));

        var result = await _sut.IsFavoriteAsync("cat1");

        result.Should().BeFalse();
    }

    #endregion

    #region SyncAsync

    [Fact]
    public async Task SyncAsync_WhenOffline_ShouldReturnErrorMessage()
    {
        // Arrange
        _mockConnectivity.Setup(c => c.IsConnected).Returns(false);

        // Act
        var result = await _sut.SyncAsync();

        // Assert
        result.ErrorMessage.Should().NotBeNull();
        result.ErrorMessage.Should().Contain("No internet");
    }

    [Fact]
    public async Task SyncAsync_WhenOnlineAndSuccessful_ShouldReturnSuccess()
    {
        // Arrange
        _mockConnectivity.Setup(c => c.IsConnected).Returns(true);
        _mockRepo.Setup(r => r.SyncFavoritesAsync(_mockApi.Object)).ReturnsAsync(true);

        // Act
        var result = await _sut.SyncAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeTrue();
    }

    [Fact]
    public async Task SyncAsync_WhenOnlineButFails_ShouldReturnErrorMessage()
    {
        // Arrange
        _mockConnectivity.Setup(c => c.IsConnected).Returns(true);
        _mockRepo.Setup(r => r.SyncFavoritesAsync(_mockApi.Object)).ReturnsAsync(false);

        // Act
        var result = await _sut.SyncAsync();

        // Assert
        result.ErrorMessage.Should().NotBeNull();
        result.ErrorMessage.Should().Contain("Sync completed with errors");
    }

    [Fact]
    public async Task SyncAsync_WhenException_ShouldReturnErrorMessage()
    {
        // Arrange
        _mockConnectivity.Setup(c => c.IsConnected).Returns(true);
        _mockRepo.Setup(r => r.SyncFavoritesAsync(_mockApi.Object))
            .ThrowsAsync(new Exception("Network error"));

        // Act
        var result = await _sut.SyncAsync();

        // Assert
        result.ErrorMessage.Should().NotBeNull();
        result.ErrorMessage.Should().Contain("Sync failed");
    }

    #endregion

    #region GetUnsyncedCountAsync

    [Fact]
    public async Task GetUnsyncedCountAsync_ShouldReturnCount()
    {
        _mockRepo.Setup(r => r.GetUnsyncedCountAsync()).ReturnsAsync(5);

        var result = await _sut.GetUnsyncedCountAsync();

        result.Should().Be(5);
    }

    [Fact]
    public async Task GetUnsyncedCountAsync_WhenException_ShouldReturnZero()
    {
        _mockRepo.Setup(r => r.GetUnsyncedCountAsync())
            .ThrowsAsync(new Exception("DB error"));

        var result = await _sut.GetUnsyncedCountAsync();

        result.Should().Be(0);
    }

    #endregion
}
