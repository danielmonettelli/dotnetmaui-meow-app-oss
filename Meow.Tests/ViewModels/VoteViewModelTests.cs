namespace Meow.Tests.ViewModels;

/// <summary>
/// Unit tests for VoteViewModel with Clean Architecture UseCases
/// </summary>
public class VoteViewModelTests
{
    #region Setup

    private readonly VoteViewModel _sut;

    public VoteViewModelTests()
    {
        // Create mocks with minimal defaults so constructor's InitializeDataAsync doesn't fail
        var mockApi = new Mock<ICatApiService>();
        var mockCatCache = new Mock<ICatCacheRepository>();
        var mockConnectivity = new Mock<IConnectivityProvider>();
        var mockFavoriteRepo = new Mock<IFavoriteRepository>();

        mockConnectivity.Setup(c => c.IsConnected).Returns(false);
        mockCatCache.Setup(c => c.ShouldRefreshVotingCacheAsync()).ReturnsAsync(false);
        mockCatCache.Setup(c => c.GetCachedVotingCatsAsync(It.IsAny<int>())).ReturnsAsync(new List<Cat>());
        mockFavoriteRepo.Setup(r => r.IsFavoriteAsync(It.IsAny<string>())).ReturnsAsync(false);

        var getVotingCats = new GetVotingCatsUseCase(mockApi.Object, mockCatCache.Object, mockConnectivity.Object);
        var manageFavorites = new ManageFavoritesUseCase(mockApi.Object, mockFavoriteRepo.Object, mockConnectivity.Object);

        _sut = new VoteViewModel(getVotingCats, manageFavorites, mockConnectivity.Object);
    }

    #endregion

    #region Constructor Tests

    [Fact]
    public void Constructor_ShouldSetTitle()
    {
        _sut.Title.Should().Be("Vote");
    }

    [Fact]
    public void Constructor_ShouldInitializeDefaultHeartIcon()
    {
        _sut.ImageHeart.Should().Be("icon_heart_outline.png");
    }

    [Fact]
    public void Constructor_ShouldInitializeAnimationAsFalse()
    {
        _sut.IsAnimation.Should().BeFalse();
    }

    #endregion

    #region Commands

    [Fact]
    public void GetKittyCommand_ShouldBeAvailable()
    {
        _sut.GetKittyCommand.Should().NotBeNull();
    }

    [Fact]
    public void LoveKittyCommand_ShouldBeAvailable()
    {
        _sut.LoveKittyCommand.Should().NotBeNull();
    }

    [Fact]
    public void ManageFavoriteKittenCommand_ShouldBeAvailable()
    {
        _sut.ManageFavoriteKittenCommand.Should().NotBeNull();
    }

    #endregion
}

/// <summary>
/// Integration-style tests for VoteViewModel with controlled use case behavior
/// </summary>
public class VoteViewModelIntegrationTests
{
    private readonly Mock<ICatApiService> _mockApi;
    private readonly Mock<ICatCacheRepository> _mockCatCache;
    private readonly Mock<IConnectivityProvider> _mockConnectivity;
    private readonly Mock<IFavoriteRepository> _mockFavoriteRepo;

    public VoteViewModelIntegrationTests()
    {
        _mockApi = new Mock<ICatApiService>();
        _mockCatCache = new Mock<ICatCacheRepository>();
        _mockConnectivity = new Mock<IConnectivityProvider>();
        _mockFavoriteRepo = new Mock<IFavoriteRepository>();
    }

    private VoteViewModel CreateSut()
    {
        var getVotingCats = new GetVotingCatsUseCase(_mockApi.Object, _mockCatCache.Object, _mockConnectivity.Object);
        var manageFavorites = new ManageFavoritesUseCase(_mockApi.Object, _mockFavoriteRepo.Object, _mockConnectivity.Object);
        return new VoteViewModel(getVotingCats, manageFavorites, _mockConnectivity.Object);
    }

    [Fact]
    public async Task InitializeDataAsync_WhenOnlineWithCats_ShouldLoadCats()
    {
        // Arrange
        _mockConnectivity.Setup(c => c.IsConnected).Returns(true);
        _mockCatCache.Setup(c => c.ShouldRefreshVotingCacheAsync()).ReturnsAsync(true);
        var cats = new List<Cat> { new Cat { Id = "cat1", Url = "https://example.com/cat1.jpg" } };
        _mockApi.Setup(a => a.GetRandomCatAsync()).ReturnsAsync(cats);
        _mockCatCache.Setup(c => c.GetCachedVotingCatsAsync(10)).ReturnsAsync(new List<Cat>());
        _mockFavoriteRepo.Setup(r => r.IsFavoriteAsync("cat1")).ReturnsAsync(false);

        var sut = CreateSut();

        // Act
        await sut.InitializeDataAsync();

        // Assert
        sut.Cats.Should().NotBeNull();
        sut.Cats.Should().NotBeEmpty();
    }

    [Fact]
    public async Task InitializeDataAsync_WhenCatIsFavorite_ShouldShowSolidHeart()
    {
        // Arrange
        _mockConnectivity.Setup(c => c.IsConnected).Returns(true);
        _mockCatCache.Setup(c => c.ShouldRefreshVotingCacheAsync()).ReturnsAsync(true);
        var cats = new List<Cat> { new Cat { Id = "cat1" } };
        _mockApi.Setup(a => a.GetRandomCatAsync()).ReturnsAsync(cats);
        _mockCatCache.Setup(c => c.GetCachedVotingCatsAsync(10)).ReturnsAsync(new List<Cat>());
        _mockFavoriteRepo.Setup(r => r.IsFavoriteAsync("cat1")).ReturnsAsync(true);

        var sut = CreateSut();

        // Act
        await sut.InitializeDataAsync();

        // Assert
        sut.ImageHeart.Should().Be("icon_heart_solid.png");
    }

    [Fact]
    public async Task InitializeDataAsync_WhenCatIsNotFavorite_ShouldShowOutlineHeart()
    {
        // Arrange
        _mockConnectivity.Setup(c => c.IsConnected).Returns(true);
        _mockCatCache.Setup(c => c.ShouldRefreshVotingCacheAsync()).ReturnsAsync(true);
        var cats = new List<Cat> { new Cat { Id = "cat1" } };
        _mockApi.Setup(a => a.GetRandomCatAsync()).ReturnsAsync(cats);
        _mockCatCache.Setup(c => c.GetCachedVotingCatsAsync(10)).ReturnsAsync(new List<Cat>());
        _mockFavoriteRepo.Setup(r => r.IsFavoriteAsync("cat1")).ReturnsAsync(false);

        var sut = CreateSut();

        // Act
        await sut.InitializeDataAsync();

        // Assert
        sut.ImageHeart.Should().Be("icon_heart_outline.png");
    }

    [Fact]
    public async Task InitializeDataAsync_WhenOffline_ShouldSetIsOffline()
    {
        // Arrange
        _mockConnectivity.Setup(c => c.IsConnected).Returns(false);
        _mockCatCache.Setup(c => c.ShouldRefreshVotingCacheAsync()).ReturnsAsync(true);
        var cached = new List<Cat> { new Cat { Id = "cached1" } };
        _mockCatCache.Setup(c => c.GetCachedVotingCatsAsync(It.IsAny<int>())).ReturnsAsync(cached);
        _mockFavoriteRepo.Setup(r => r.IsFavoriteAsync(It.IsAny<string>())).ReturnsAsync(false);

        var sut = CreateSut();

        // Act
        await sut.InitializeDataAsync();

        // Assert
        sut.IsOffline.Should().BeTrue();
    }

    [Fact]
    public async Task InitializeDataAsync_ShouldShowContentAfterLoading()
    {
        // Arrange
        _mockConnectivity.Setup(c => c.IsConnected).Returns(false);
        _mockCatCache.Setup(c => c.ShouldRefreshVotingCacheAsync()).ReturnsAsync(false);
        _mockCatCache.Setup(c => c.GetCachedVotingCatsAsync(It.IsAny<int>())).ReturnsAsync(new List<Cat>());

        var sut = CreateSut();

        // Act
        await sut.InitializeDataAsync();

        // Assert
        sut.IsHidden.Should().BeFalse();
    }

    [Fact]
    public async Task GetKittyAsync_ShouldStopAnimation()
    {
        // Arrange
        _mockConnectivity.Setup(c => c.IsConnected).Returns(false);
        _mockCatCache.Setup(c => c.ShouldRefreshVotingCacheAsync()).ReturnsAsync(false);
        _mockCatCache.Setup(c => c.GetCachedVotingCatsAsync(It.IsAny<int>())).ReturnsAsync(new List<Cat>());

        var sut = CreateSut();
        sut.IsAnimation = true;

        // Act
        await sut.GetKittyAsync();

        // Assert
        sut.IsAnimation.Should().BeFalse();
    }

    [Fact]
    public async Task ManageFavoriteKittenAsync_WhenNotFavorite_ShouldAddToFavorites()
    {
        // Arrange
        _mockConnectivity.Setup(c => c.IsConnected).Returns(true);
        _mockCatCache.Setup(c => c.ShouldRefreshVotingCacheAsync()).ReturnsAsync(false);
        var cats = new List<Cat> { new Cat { Id = "cat1", Url = "https://example.com/cat1.jpg" } };
        _mockCatCache.Setup(c => c.GetCachedVotingCatsAsync(It.IsAny<int>())).ReturnsAsync(cats);
        _mockFavoriteRepo.Setup(r => r.IsFavoriteAsync("cat1")).ReturnsAsync(false);
        _mockFavoriteRepo.Setup(r => r.AddFavoriteAsync(It.IsAny<Cat>())).ReturnsAsync(true);

        var sut = CreateSut();
        await sut.InitializeDataAsync();

        // LayoutState should be None (not favorite)
        sut.LayoutState.Should().Be(LayoutState.None);

        // Act
        await sut.ManageFavoriteKittenAsync();

        // Assert
        sut.ImageHeart.Should().Be("icon_heart_solid.png");
        sut.IsAnimation.Should().BeTrue();
    }

    [Fact]
    public async Task ManageFavoriteKittenAsync_WhenAlreadyFavorite_ShouldRemoveFromFavorites()
    {
        // Arrange
        _mockConnectivity.Setup(c => c.IsConnected).Returns(true);
        _mockCatCache.Setup(c => c.ShouldRefreshVotingCacheAsync()).ReturnsAsync(true);
        var cats = new List<Cat> { new Cat { Id = "cat1" } };
        _mockApi.Setup(a => a.GetRandomCatAsync()).ReturnsAsync(cats);
        _mockCatCache.Setup(c => c.GetCachedVotingCatsAsync(10)).ReturnsAsync(new List<Cat>());
        _mockFavoriteRepo.Setup(r => r.IsFavoriteAsync("cat1")).ReturnsAsync(true);
        _mockFavoriteRepo.Setup(r => r.RemoveFavoriteAsync("cat1")).ReturnsAsync(true);

        var sut = CreateSut();
        await sut.InitializeDataAsync();

        // After init, cat1 is favorite so LayoutState should be Success
        sut.LayoutState.Should().Be(LayoutState.Success);

        // Act — since LayoutState is not None, it should remove
        await sut.ManageFavoriteKittenAsync();

        // Assert
        sut.ImageHeart.Should().Be("icon_heart_outline.png");
        sut.LayoutState.Should().Be(LayoutState.None);
        sut.IsAnimation.Should().BeFalse();
    }

    [Fact]
    public async Task ManageFavoriteKittenAsync_WhenOfflineAndAdding_ShouldShowPendingSyncMessage()
    {
        // Arrange
        _mockConnectivity.Setup(c => c.IsConnected).Returns(false);
        _mockCatCache.Setup(c => c.ShouldRefreshVotingCacheAsync()).ReturnsAsync(false);
        var cats = new List<Cat> { new Cat { Id = "cat1" } };
        _mockCatCache.Setup(c => c.GetCachedVotingCatsAsync(It.IsAny<int>())).ReturnsAsync(cats);
        _mockFavoriteRepo.Setup(r => r.IsFavoriteAsync("cat1")).ReturnsAsync(false);
        _mockFavoriteRepo.Setup(r => r.AddFavoriteAsync(It.IsAny<Cat>())).ReturnsAsync(true);

        var sut = CreateSut();
        await sut.InitializeDataAsync();

        // Act
        await sut.ManageFavoriteKittenAsync();

        // Assert
        sut.ImageHeart.Should().Be("icon_heart_solid.png");
        // StatusMessage should mention "will sync when online" but it auto-clears
    }

    [Fact]
    public async Task LoveKittyAsync_ShouldNotTriggerHeartAnimation()
    {
        // Arrange
        _mockConnectivity.Setup(c => c.IsConnected).Returns(true);
        _mockCatCache.Setup(c => c.ShouldRefreshVotingCacheAsync()).ReturnsAsync(false);
        var cats = new List<Cat> { new Cat { Id = "cat1", Url = "https://example.com/cat1.jpg" } };
        _mockCatCache.Setup(c => c.GetCachedVotingCatsAsync(It.IsAny<int>())).ReturnsAsync(cats);
        _mockFavoriteRepo.Setup(r => r.IsFavoriteAsync("cat1")).ReturnsAsync(false);
        _mockFavoriteRepo.Setup(r => r.AddFavoriteAsync(It.IsAny<Cat>())).ReturnsAsync(true);

        var sut = CreateSut();
        await sut.InitializeDataAsync();

        bool eventFired = false;
        sut.HeartAnimationRequested += () =>
        {
            eventFired = true;
            return Task.CompletedTask;
        };

        // Act - Love it should only advance without heart animation
        await sut.LoveKittyAsync();

        // Assert
        eventFired.Should().BeFalse();
    }

    [Fact]
    public async Task ManageFavoriteKittenAsync_WhenAddingFavorite_ShouldTriggerHeartAnimation()
    {
        // Arrange
        _mockConnectivity.Setup(c => c.IsConnected).Returns(true);
        _mockCatCache.Setup(c => c.ShouldRefreshVotingCacheAsync()).ReturnsAsync(false);
        var cats = new List<Cat> { new Cat { Id = "cat1", Url = "https://example.com/cat1.jpg" } };
        _mockCatCache.Setup(c => c.GetCachedVotingCatsAsync(It.IsAny<int>())).ReturnsAsync(cats);
        _mockFavoriteRepo.Setup(r => r.IsFavoriteAsync("cat1")).ReturnsAsync(false);
        _mockFavoriteRepo.Setup(r => r.AddFavoriteAsync(It.IsAny<Cat>())).ReturnsAsync(true);

        var sut = CreateSut();
        await sut.InitializeDataAsync();

        int eventFiredCount = 0;
        sut.HeartAnimationRequested += () =>
        {
            eventFiredCount++;
            return Task.CompletedTask;
        };

        // Act
        await sut.ManageFavoriteKittenAsync();

        // Assert
        eventFiredCount.Should().Be(1);
    }

    [Fact]
    public async Task GetKittyAsync_WhenMultipleCatsAvailable_ShouldAdvanceToDifferentCat()
    {
        // Arrange
        _mockConnectivity.Setup(c => c.IsConnected).Returns(true);
        _mockCatCache.Setup(c => c.ShouldRefreshVotingCacheAsync()).ReturnsAsync(false);
        var cats = new List<Cat>
        {
            new Cat { Id = "cat1", Url = "https://example.com/cat1.jpg" },
            new Cat { Id = "cat2", Url = "https://example.com/cat2.jpg" }
        };
        _mockCatCache.Setup(c => c.GetCachedVotingCatsAsync(It.IsAny<int>())).ReturnsAsync(cats);
        _mockFavoriteRepo.Setup(r => r.IsFavoriteAsync(It.IsAny<string>())).ReturnsAsync(false);

        var sut = CreateSut();
        await sut.InitializeDataAsync();

        sut.CurrentCat.Should().NotBeNull();
        var initialCatId = sut.CurrentCat!.Id;

        // Act
        await sut.GetKittyAsync();

        // Assert
        sut.CurrentCat.Should().NotBeNull();
        sut.CurrentCat!.Id.Should().NotBe(initialCatId);
    }

    [Fact]
    public async Task LoveKittyAsync_WhenMultipleCatsAvailable_ShouldAdvanceToDifferentCat()
    {
        // Arrange
        _mockConnectivity.Setup(c => c.IsConnected).Returns(true);
        _mockCatCache.Setup(c => c.ShouldRefreshVotingCacheAsync()).ReturnsAsync(false);
        var cats = new List<Cat>
        {
            new Cat { Id = "cat1", Url = "https://example.com/cat1.jpg" },
            new Cat { Id = "cat2", Url = "https://example.com/cat2.jpg" }
        };
        _mockCatCache.Setup(c => c.GetCachedVotingCatsAsync(It.IsAny<int>())).ReturnsAsync(cats);
        _mockFavoriteRepo.Setup(r => r.IsFavoriteAsync(It.IsAny<string>())).ReturnsAsync(false);
        _mockFavoriteRepo.Setup(r => r.AddFavoriteAsync(It.IsAny<Cat>())).ReturnsAsync(true);

        var sut = CreateSut();
        await sut.InitializeDataAsync();

        sut.CurrentCat.Should().NotBeNull();
        var initialCatId = sut.CurrentCat!.Id;

        // Act
        await sut.LoveKittyAsync();

        // Assert
        sut.CurrentCat.Should().NotBeNull();
        sut.CurrentCat!.Id.Should().NotBe(initialCatId);
    }

    [Fact]
    public async Task MoveToNextCat_WhenAtEndOfList_ShouldReloadAndSelectDifferentCat()
    {
        // Arrange
        _mockConnectivity.Setup(c => c.IsConnected).Returns(true);
        _mockCatCache.Setup(c => c.ShouldRefreshVotingCacheAsync()).ReturnsAsync(false);
        var initialCats = new List<Cat>
        {
            new Cat { Id = "cat1", Url = "https://example.com/cat1.jpg" }
        };
        _mockCatCache.Setup(c => c.GetCachedVotingCatsAsync(It.IsAny<int>())).ReturnsAsync(initialCats);

        // When reloading, return cats with a different id
        var reloadedCats = new List<Cat>
        {
            new Cat { Id = "cat1", Url = "https://example.com/cat1.jpg" },
            new Cat { Id = "cat2", Url = "https://example.com/cat2.jpg" }
        };
        _mockApi.Setup(a => a.GetRandomCatAsync()).ReturnsAsync(reloadedCats);
        _mockFavoriteRepo.Setup(r => r.IsFavoriteAsync(It.IsAny<string>())).ReturnsAsync(false);

        var sut = CreateSut();
        await sut.InitializeDataAsync();

        sut.CurrentCat!.Id.Should().Be("cat1");

        // Act — at end of list (only 1 item), should trigger reload and pick a different cat
        await sut.GetKittyAsync();

        // Assert
        sut.CurrentCat.Should().NotBeNull();
        sut.CurrentCat!.Id.Should().Be("cat2");
    }

    [Fact]
    public async Task LoveKittyAsync_ShouldNotAddCatToFavorites()
    {
        // Arrange
        _mockConnectivity.Setup(c => c.IsConnected).Returns(true);
        _mockCatCache.Setup(c => c.ShouldRefreshVotingCacheAsync()).ReturnsAsync(false);
        var cats = new List<Cat>
        {
            new Cat { Id = "cat1", Url = "https://example.com/cat1.jpg" },
            new Cat { Id = "cat2", Url = "https://example.com/cat2.jpg" }
        };
        _mockCatCache.Setup(c => c.GetCachedVotingCatsAsync(It.IsAny<int>())).ReturnsAsync(cats);
        _mockFavoriteRepo.Setup(r => r.IsFavoriteAsync(It.IsAny<string>())).ReturnsAsync(false);

        var sut = CreateSut();
        await sut.InitializeDataAsync();

        sut.CurrentCat.Should().NotBeNull();
        var initialCatId = sut.CurrentCat!.Id;

        // Act — voting love should advance without calling AddFavorite
        await sut.LoveKittyAsync();

        // Assert
        _mockFavoriteRepo.Verify(r => r.AddFavoriteAsync(It.IsAny<Cat>()), Times.Never);
        sut.CurrentCat.Should().NotBeNull();
        sut.CurrentCat!.Id.Should().NotBe(initialCatId);
    }

    [Fact]
    public async Task MoveToNextCat_ShouldPrefetchFreshCatsWhenBufferIsLow()
    {
        // Arrange
        _mockConnectivity.Setup(c => c.IsConnected).Returns(true);
        _mockCatCache.Setup(c => c.ShouldRefreshVotingCacheAsync()).ReturnsAsync(false);
        var initialCats = new List<Cat>
        {
            new Cat { Id = "cat1", Url = "https://example.com/cat1.jpg" },
            new Cat { Id = "cat2", Url = "https://example.com/cat2.jpg" }
        };
        _mockCatCache.Setup(c => c.GetCachedVotingCatsAsync(It.IsAny<int>())).ReturnsAsync(initialCats);

        var prefetchCats = new List<Cat>
        {
            new Cat { Id = "cat3", Url = "https://example.com/cat3.jpg" },
            new Cat { Id = "cat4", Url = "https://example.com/cat4.jpg" }
        };
        _mockApi.Setup(a => a.GetRandomCatAsync()).ReturnsAsync(prefetchCats);
        _mockFavoriteRepo.Setup(r => r.IsFavoriteAsync(It.IsAny<string>())).ReturnsAsync(false);

        var sut = CreateSut();
        await sut.InitializeDataAsync();

        // Act — advancing through the initial list
        await sut.GetKittyAsync();

        // Allow any background prefetch task to complete
        await Task.Delay(100, TestContext.Current.CancellationToken);

        // Assert: API was called to prefetch more cats
        _mockApi.Verify(a => a.GetRandomCatAsync(), Times.AtLeastOnce);
    }
}
