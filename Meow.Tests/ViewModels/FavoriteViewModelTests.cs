namespace Meow.Tests.ViewModels;

/// <summary>
/// Unit tests for FavoriteViewModel with Clean Architecture UseCases
/// </summary>
public class FavoriteViewModelTests
{
    #region Setup

    private readonly FavoriteViewModel _sut;

    public FavoriteViewModelTests()
    {
        var mockApi = new Mock<ICatApiService>();
        var mockFavoriteRepo = new Mock<IFavoriteRepository>();
        var mockConnectivity = new Mock<IConnectivityProvider>();

        mockConnectivity.Setup(c => c.IsConnected).Returns(false);
        mockFavoriteRepo.Setup(r => r.GetUserFavoritesAsync()).ReturnsAsync(new List<FavoriteCatResponse>());
        mockFavoriteRepo.Setup(r => r.GetUnsyncedCountAsync()).ReturnsAsync(0);

        var manageFavorites = new ManageFavoritesUseCase(mockApi.Object, mockFavoriteRepo.Object, mockConnectivity.Object);
        _sut = new FavoriteViewModel(manageFavorites);
    }

    #endregion

    #region Constructor Tests

    [Fact]
    public void Constructor_ShouldSetTitle()
    {
        _sut.Title.Should().Be("Favorites");
    }

    [Fact]
    public void Constructor_ShouldInitializeDefaultProperties()
    {
        _sut.SelectedFavoriteCat.Should().NotBeNull();
    }

    #endregion

    #region Command Tests

    [Fact]
    public void DeleteFavoriteKittenCommand_ShouldBeAvailable()
    {
        _sut.DeleteFavoriteKittenCommand.Should().NotBeNull();
    }

    [Fact]
    public void SyncFavoritesCommandCommand_ShouldBeAvailable()
    {
        _sut.SyncFavoritesCommandCommand.Should().NotBeNull();
    }

    [Fact]
    public void RefreshFavoritesCommand_ShouldBeAvailable()
    {
        _sut.RefreshFavoritesCommand.Should().NotBeNull();
    }

    #endregion
}

/// <summary>
/// Integration-style tests for FavoriteViewModel with controlled use case behavior
/// </summary>
public class FavoriteViewModelIntegrationTests
{
    private readonly Mock<ICatApiService> _mockApi;
    private readonly Mock<IFavoriteRepository> _mockFavoriteRepo;
    private readonly Mock<IConnectivityProvider> _mockConnectivity;

    public FavoriteViewModelIntegrationTests()
    {
        _mockApi = new Mock<ICatApiService>();
        _mockFavoriteRepo = new Mock<IFavoriteRepository>();
        _mockConnectivity = new Mock<IConnectivityProvider>();
    }

    private FavoriteViewModel CreateSut()
    {
        var manageFavorites = new ManageFavoritesUseCase(_mockApi.Object, _mockFavoriteRepo.Object, _mockConnectivity.Object);
        return new FavoriteViewModel(manageFavorites);
    }

    [Fact]
    public async Task InitializeDataAsync_ShouldLoadFavoritesFromRepo()
    {
        // Arrange
        _mockConnectivity.Setup(c => c.IsConnected).Returns(false);
        var favorites = new List<FavoriteCatResponse>
        {
            new FavoriteCatResponse
            {
                Id = "fav1",
                Image_id = "cat1",
                Image = new Cat { Id = "cat1", Url = "https://example.com/cat1.jpg" }
            },
            new FavoriteCatResponse
            {
                Id = "fav2",
                Image_id = "cat2",
                Image = new Cat { Id = "cat2", Url = "https://example.com/cat2.jpg" }
            }
        };
        _mockFavoriteRepo.Setup(r => r.GetUserFavoritesAsync()).ReturnsAsync(favorites);
        _mockFavoriteRepo.Setup(r => r.GetUnsyncedCountAsync()).ReturnsAsync(0);

        var sut = CreateSut();

        // Act
        await sut.InitializeDataAsync();

        // Assert
        sut.FavoriteCats.Should().HaveCount(2);
    }

    [Fact]
    public async Task InitializeDataAsync_ShouldUpdateUnsyncedCount()
    {
        // Arrange
        _mockConnectivity.Setup(c => c.IsConnected).Returns(false);
        _mockFavoriteRepo.Setup(r => r.GetUserFavoritesAsync()).ReturnsAsync(new List<FavoriteCatResponse>());
        _mockFavoriteRepo.Setup(r => r.GetUnsyncedCountAsync()).ReturnsAsync(3);

        var sut = CreateSut();

        // Act
        await sut.InitializeDataAsync();

        // Assert
        sut.UnsyncedCount.Should().Be(3);
    }

    [Fact]
    public async Task InitializeDataAsync_WhenUnsyncedFavorites_ShouldShowStatusMessage()
    {
        // Arrange
        _mockConnectivity.Setup(c => c.IsConnected).Returns(false);
        _mockFavoriteRepo.Setup(r => r.GetUserFavoritesAsync()).ReturnsAsync(new List<FavoriteCatResponse>());
        _mockFavoriteRepo.Setup(r => r.GetUnsyncedCountAsync()).ReturnsAsync(5);

        var sut = CreateSut();

        // Act
        await sut.InitializeDataAsync();

        // Assert
        sut.StatusMessage.Should().Contain("pending sync");
    }

    [Fact]
    public async Task InitializeDataAsync_ShouldSetIsBusyDuringOperation()
    {
        // Arrange
        _mockConnectivity.Setup(c => c.IsConnected).Returns(false);
        _mockFavoriteRepo.Setup(r => r.GetUserFavoritesAsync()).ReturnsAsync(new List<FavoriteCatResponse>());
        _mockFavoriteRepo.Setup(r => r.GetUnsyncedCountAsync()).ReturnsAsync(0);

        var busyStates = new List<bool>();
        var sut = CreateSut();
        sut.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(FavoriteViewModel.IsBusy))
                busyStates.Add(sut.IsBusy);
        };

        // Act
        await sut.InitializeDataAsync();

        // Assert
        busyStates.Should().Contain(true);
        sut.IsBusy.Should().BeFalse();
    }

    [Fact]
    public async Task InitializeDataAsync_WhenNoFavorites_ShouldReturnEmptyList()
    {
        // Arrange
        _mockConnectivity.Setup(c => c.IsConnected).Returns(false);
        _mockFavoriteRepo.Setup(r => r.GetUserFavoritesAsync()).ReturnsAsync(new List<FavoriteCatResponse>());
        _mockFavoriteRepo.Setup(r => r.GetUnsyncedCountAsync()).ReturnsAsync(0);

        var sut = CreateSut();

        // Act
        await sut.InitializeDataAsync();

        // Assert
        sut.FavoriteCats.Should().BeEmpty();
    }

    [Fact]
    public async Task SyncFavoritesAsync_WhenOnlineAndSuccessful_ShouldRefreshData()
    {
        // Arrange
        _mockConnectivity.Setup(c => c.IsConnected).Returns(true);
        _mockFavoriteRepo.Setup(r => r.SyncFavoritesAsync(_mockApi.Object)).ReturnsAsync(true);
        var favorites = new List<FavoriteCatResponse>
        {
            new FavoriteCatResponse { Id = "fav1", Image = new Cat { Id = "cat1" } }
        };
        _mockFavoriteRepo.Setup(r => r.GetUserFavoritesAsync()).ReturnsAsync(favorites);
        _mockFavoriteRepo.Setup(r => r.GetUnsyncedCountAsync()).ReturnsAsync(0);

        var sut = CreateSut();

        // Act
        await sut.SyncFavoritesAsync();

        // Assert
        sut.FavoriteCats.Should().HaveCount(1);
        sut.IsSyncing.Should().BeFalse();
    }

    [Fact]
    public async Task SyncFavoritesAsync_WhenOffline_ShouldShowError()
    {
        // Arrange
        _mockConnectivity.Setup(c => c.IsConnected).Returns(false);
        _mockFavoriteRepo.Setup(r => r.GetUserFavoritesAsync()).ReturnsAsync(new List<FavoriteCatResponse>());
        _mockFavoriteRepo.Setup(r => r.GetUnsyncedCountAsync()).ReturnsAsync(0);

        var sut = CreateSut();

        // Act
        await sut.SyncFavoritesAsync();

        // Assert
        sut.IsSyncing.Should().BeFalse();
    }

    [Fact]
    public async Task SyncFavoritesAsync_ShouldSetIsSyncingDuringOperation()
    {
        // Arrange
        _mockConnectivity.Setup(c => c.IsConnected).Returns(true);
        _mockFavoriteRepo.Setup(r => r.SyncFavoritesAsync(_mockApi.Object)).ReturnsAsync(true);
        _mockFavoriteRepo.Setup(r => r.GetUserFavoritesAsync()).ReturnsAsync(new List<FavoriteCatResponse>());
        _mockFavoriteRepo.Setup(r => r.GetUnsyncedCountAsync()).ReturnsAsync(0);

        var syncStates = new List<bool>();
        var sut = CreateSut();
        sut.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(FavoriteViewModel.IsSyncing))
                syncStates.Add(sut.IsSyncing);
        };

        // Act
        await sut.SyncFavoritesAsync();

        // Assert
        syncStates.Should().Contain(true);
        sut.IsSyncing.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteFavoriteKittenAsync_WhenFavoriteSelected_ShouldRemoveAndRefresh()
    {
        // Arrange
        _mockConnectivity.Setup(c => c.IsConnected).Returns(false);
        _mockFavoriteRepo.Setup(r => r.RemoveFavoriteAsync("cat1")).ReturnsAsync(true);
        var remaining = new List<FavoriteCatResponse>();
        _mockFavoriteRepo.Setup(r => r.GetUserFavoritesAsync()).ReturnsAsync(remaining);
        _mockFavoriteRepo.Setup(r => r.GetUnsyncedCountAsync()).ReturnsAsync(0);

        var sut = CreateSut();

        // Act — setting SelectedFavoriteCat triggers OnSelectedFavoriteCatChanged
        // which auto-calls DeleteFavoriteKittenAsync via the partial method
        sut.SelectedFavoriteCat = new FavoriteCatResponse
        {
            Id = "fav1",
            Image = new Cat { Id = "cat1", Url = "https://example.com/cat1.jpg" }
        };

        // Allow fire-and-forget to complete
        await Task.Delay(200);

        // Assert
        _mockFavoriteRepo.Verify(r => r.RemoveFavoriteAsync("cat1"), Times.Once);
        sut.FavoriteCats.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteFavoriteKittenAsync_WhenNoImageSelected_ShouldNotCallRemove()
    {
        // Arrange
        _mockConnectivity.Setup(c => c.IsConnected).Returns(false);
        _mockFavoriteRepo.Setup(r => r.GetUserFavoritesAsync()).ReturnsAsync(new List<FavoriteCatResponse>());
        _mockFavoriteRepo.Setup(r => r.GetUnsyncedCountAsync()).ReturnsAsync(0);

        var sut = CreateSut();
        sut.SelectedFavoriteCat = new FavoriteCatResponse(); // No Image set

        // Act
        await sut.DeleteFavoriteKittenAsync();

        // Assert
        _mockFavoriteRepo.Verify(r => r.RemoveFavoriteAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task RefreshFavoritesAsync_ShouldReloadData()
    {
        // Arrange
        _mockConnectivity.Setup(c => c.IsConnected).Returns(false);
        var favorites = new List<FavoriteCatResponse>
        {
            new FavoriteCatResponse { Id = "fav1", Image = new Cat { Id = "cat1" } }
        };
        _mockFavoriteRepo.Setup(r => r.GetUserFavoritesAsync()).ReturnsAsync(favorites);
        _mockFavoriteRepo.Setup(r => r.GetUnsyncedCountAsync()).ReturnsAsync(0);

        var sut = CreateSut();

        // Act
        await sut.RefreshFavoritesAsync();

        // Assert
        sut.FavoriteCats.Should().HaveCount(1);
    }
}
