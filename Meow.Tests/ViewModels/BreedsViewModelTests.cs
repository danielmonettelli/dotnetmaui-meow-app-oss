namespace Meow.Tests.ViewModels;

/// <summary>
/// Unit tests for BreedsViewModel with Clean Architecture UseCases
/// </summary>
public class BreedsViewModelTests
{
    #region Setup

    private readonly BreedsViewModel _sut;

    public BreedsViewModelTests()
    {
        var mockApi = new Mock<ICatApiService>();
        var mockCatCache = new Mock<ICatCacheRepository>();
        var mockBreedCache = new Mock<IBreedCacheRepository>();
        var mockConnectivity = new Mock<IConnectivityProvider>();

        mockConnectivity.Setup(c => c.IsConnected).Returns(false);
        mockBreedCache.Setup(c => c.IsBreedCacheValidAsync()).ReturnsAsync(false);
        mockBreedCache.Setup(c => c.GetCachedBreedsAsync()).ReturnsAsync(new List<Breed>());
        mockCatCache.Setup(c => c.GetCachedCatsByBreedAsync(It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync(new List<Cat>());

        var getBreedsUseCase = new GetBreedsUseCase(mockApi.Object, mockBreedCache.Object, mockConnectivity.Object);
        var getCatsByBreedUseCase = new GetCatsByBreedUseCase(mockApi.Object, mockCatCache.Object, mockConnectivity.Object);

        _sut = new BreedsViewModel(getBreedsUseCase, getCatsByBreedUseCase);
    }

    #endregion

    #region Constructor Tests

    [Fact]
    public void Constructor_ShouldSetTitle()
    {
        _sut.Title.Should().Be("Breeds");
    }

    #endregion

    #region Command Tests

    [Fact]
    public void SelectedBreed_PropertyShouldExist()
    {
        _sut.SelectedBreed.Should().BeNull();
    }

    #endregion
}

/// <summary>
/// Integration-style tests for BreedsViewModel with controlled use case behavior
/// </summary>
public class BreedsViewModelIntegrationTests
{
    private readonly Mock<ICatApiService> _mockApi;
    private readonly Mock<ICatCacheRepository> _mockCatCache;
    private readonly Mock<IBreedCacheRepository> _mockBreedCache;
    private readonly Mock<IConnectivityProvider> _mockConnectivity;

    public BreedsViewModelIntegrationTests()
    {
        _mockApi = new Mock<ICatApiService>();
        _mockCatCache = new Mock<ICatCacheRepository>();
        _mockBreedCache = new Mock<IBreedCacheRepository>();
        _mockConnectivity = new Mock<IConnectivityProvider>();
    }

    private BreedsViewModel CreateSut()
    {
        var getBreedsUseCase = new GetBreedsUseCase(_mockApi.Object, _mockBreedCache.Object, _mockConnectivity.Object);
        var getCatsByBreedUseCase = new GetCatsByBreedUseCase(_mockApi.Object, _mockCatCache.Object, _mockConnectivity.Object);
        return new BreedsViewModel(getBreedsUseCase, getCatsByBreedUseCase);
    }

    [Fact]
    public async Task InitializeDataAsync_ShouldLoadBreedsFromApi()
    {
        // Arrange
        _mockConnectivity.Setup(c => c.IsConnected).Returns(true);
        _mockBreedCache.Setup(c => c.IsBreedCacheValidAsync()).ReturnsAsync(false);
        var breeds = new List<Breed>
        {
            new Breed { Id = "abys", Name = "Abyssinian" },
            new Breed { Id = "aege", Name = "Aegean" }
        };
        _mockApi.Setup(a => a.GetBreedsAsync()).ReturnsAsync(breeds);

        var cats = new List<Cat> { new Cat { Id = "cat1" } };
        _mockCatCache.Setup(c => c.GetCachedCatsByBreedAsync("abys", 10)).ReturnsAsync(new List<Cat>());
        _mockApi.Setup(a => a.GetCatsByBreedAsync("abys")).ReturnsAsync(cats);

        var sut = CreateSut();

        // Act
        await sut.InitializeDataAsync();

        // Assert
        sut.Breeds.Should().NotBeNull();
        sut.Breeds.Should().HaveCount(2);
        sut.Breeds!.First().Name.Should().Be("Abyssinian");
    }

    [Fact]
    public async Task InitializeDataAsync_ShouldLoadBreedsFromCache()
    {
        // Arrange
        var breeds = new List<Breed>
        {
            new Breed { Id = "abys", Name = "Abyssinian" },
            new Breed { Id = "aege", Name = "Aegean" }
        };
        _mockBreedCache.Setup(c => c.IsBreedCacheValidAsync()).ReturnsAsync(true);
        _mockBreedCache.Setup(c => c.GetCachedBreedsAsync()).ReturnsAsync(breeds);
        _mockCatCache.Setup(c => c.GetCachedCatsByBreedAsync("abys", 10)).ReturnsAsync(new List<Cat>());

        var sut = CreateSut();

        // Act
        await sut.InitializeDataAsync();

        // Assert
        sut.Breeds.Should().HaveCount(2);
        _mockApi.Verify(a => a.GetBreedsAsync(), Times.Never);
    }

    [Fact]
    public async Task InitializeDataAsync_ShouldSelectFirstBreed()
    {
        // Arrange
        var breeds = new List<Breed>
        {
            new Breed { Id = "abys", Name = "Abyssinian" },
            new Breed { Id = "aege", Name = "Aegean" }
        };
        _mockBreedCache.Setup(c => c.IsBreedCacheValidAsync()).ReturnsAsync(true);
        _mockBreedCache.Setup(c => c.GetCachedBreedsAsync()).ReturnsAsync(breeds);
        _mockCatCache.Setup(c => c.GetCachedCatsByBreedAsync("abys", 10)).ReturnsAsync(new List<Cat>());

        var sut = CreateSut();

        // Act
        await sut.InitializeDataAsync();

        // Assert
        sut.SelectedBreed.Should().NotBeNull();
        sut.SelectedBreed!.Id.Should().Be("abys");
    }

    [Fact]
    public async Task InitializeDataAsync_ShouldSetIsBusyDuringOperation()
    {
        // Arrange
        var breeds = new List<Breed> { new Breed { Id = "abys", Name = "Abyssinian" } };
        _mockBreedCache.Setup(c => c.IsBreedCacheValidAsync()).ReturnsAsync(true);
        _mockBreedCache.Setup(c => c.GetCachedBreedsAsync()).ReturnsAsync(breeds);
        _mockCatCache.Setup(c => c.GetCachedCatsByBreedAsync("abys", 10)).ReturnsAsync(new List<Cat>());

        var busyStates = new List<bool>();
        var sut = CreateSut();
        sut.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(BreedsViewModel.IsBusy))
                busyStates.Add(sut.IsBusy);
        };

        // Act
        await sut.InitializeDataAsync();

        // Assert
        busyStates.Should().Contain(true);
        sut.IsBusy.Should().BeFalse();
    }

    [Fact]
    public async Task InitializeDataAsync_WhenNoBreedsAvailable_ShouldHandleGracefully()
    {
        // Arrange
        _mockConnectivity.Setup(c => c.IsConnected).Returns(false);
        _mockBreedCache.Setup(c => c.IsBreedCacheValidAsync()).ReturnsAsync(false);
        _mockBreedCache.Setup(c => c.GetCachedBreedsAsync()).ReturnsAsync(new List<Breed>());

        var sut = CreateSut();

        // Act
        await sut.InitializeDataAsync();

        // Assert
        sut.SelectedBreed.Should().BeNull();
    }

    [Fact]
    public async Task InitializeDataAsync_WhenOffline_ShouldSetIsOffline()
    {
        // Arrange
        _mockConnectivity.Setup(c => c.IsConnected).Returns(false);
        _mockBreedCache.Setup(c => c.IsBreedCacheValidAsync()).ReturnsAsync(false);
        var stale = new List<Breed> { new Breed { Id = "abys", Name = "Abyssinian" } };
        _mockBreedCache.Setup(c => c.GetCachedBreedsAsync()).ReturnsAsync(stale);
        _mockCatCache.Setup(c => c.GetCachedCatsByBreedAsync("abys", 10)).ReturnsAsync(new List<Cat>());

        var sut = CreateSut();

        // Act
        await sut.InitializeDataAsync();

        // Assert
        sut.IsOffline.Should().BeTrue();
    }

    [Fact]
    public async Task SelectedBreedAsync_ShouldLoadCatsForBreed()
    {
        // Arrange
        _mockConnectivity.Setup(c => c.IsConnected).Returns(true);
        var cats = new List<Cat>
        {
            new Cat { Id = "cat1", Url = "https://example.com/bengal1.jpg" },
            new Cat { Id = "cat2", Url = "https://example.com/bengal2.jpg" }
        };
        _mockCatCache.Setup(c => c.GetCachedCatsByBreedAsync("beng", 10)).ReturnsAsync(new List<Cat>());
        _mockApi.Setup(a => a.GetCatsByBreedAsync("beng")).ReturnsAsync(cats);

        // Need to set up breeds first
        _mockBreedCache.Setup(c => c.IsBreedCacheValidAsync()).ReturnsAsync(false);
        _mockBreedCache.Setup(c => c.GetCachedBreedsAsync()).ReturnsAsync(new List<Breed>());
        var sut = CreateSut();

        // Act
        await sut.SelectedBreedAsync("beng");

        // Assert
        sut.KittensByBreed.Should().NotBeNull();
        sut.KittensByBreed.Should().HaveCount(2);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task SelectedBreedAsync_WithEmptyId_ShouldNotLoadCats(string? id)
    {
        // Arrange
        _mockBreedCache.Setup(c => c.IsBreedCacheValidAsync()).ReturnsAsync(false);
        _mockBreedCache.Setup(c => c.GetCachedBreedsAsync()).ReturnsAsync(new List<Breed>());
        var sut = CreateSut();

        // Act
        await sut.SelectedBreedAsync(id!);

        // Assert
        _mockApi.Verify(a => a.GetCatsByBreedAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task SelectedBreedAsync_ShouldSetIsLoadBreedsDuringOperation()
    {
        // Arrange
        _mockConnectivity.Setup(c => c.IsConnected).Returns(true);
        _mockCatCache.Setup(c => c.GetCachedCatsByBreedAsync("beng", 10)).ReturnsAsync(new List<Cat>());
        _mockApi.Setup(a => a.GetCatsByBreedAsync("beng")).ReturnsAsync(new List<Cat>());

        _mockBreedCache.Setup(c => c.IsBreedCacheValidAsync()).ReturnsAsync(false);
        _mockBreedCache.Setup(c => c.GetCachedBreedsAsync()).ReturnsAsync(new List<Breed>());
        var sut = CreateSut();

        var loadStates = new List<bool>();
        sut.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(BreedsViewModel.IsLoadBreeds))
                loadStates.Add(sut.IsLoadBreeds);
        };

        // Act
        await sut.SelectedBreedAsync("beng");

        // Assert
        loadStates.Should().Contain(true);
        sut.IsLoadBreeds.Should().BeFalse();
    }

    [Fact]
    public async Task SelectedBreedAsync_WhenNoCatsFound_ShouldSetHasNoKittensTrue()
    {
        // Arrange
        _mockConnectivity.Setup(c => c.IsConnected).Returns(true);
        _mockCatCache.Setup(c => c.GetCachedCatsByBreedAsync("beng", 10)).ReturnsAsync(new List<Cat>());
        _mockApi.Setup(a => a.GetCatsByBreedAsync("beng")).ReturnsAsync(new List<Cat>());

        _mockBreedCache.Setup(c => c.IsBreedCacheValidAsync()).ReturnsAsync(false);
        _mockBreedCache.Setup(c => c.GetCachedBreedsAsync()).ReturnsAsync(new List<Breed>());
        var sut = CreateSut();

        // Act
        await sut.SelectedBreedAsync("beng");

        // Assert
        sut.HasNoKittens.Should().BeTrue();
    }

    [Fact]
    public async Task SelectedBreedAsync_WhenCatsFound_ShouldSetHasNoKittensFalse()
    {
        // Arrange
        _mockConnectivity.Setup(c => c.IsConnected).Returns(true);
        var cats = new List<Cat> { new Cat { Id = "cat1", Url = "https://example.com/cat1.jpg" } };
        _mockCatCache.Setup(c => c.GetCachedCatsByBreedAsync("beng", 10)).ReturnsAsync(new List<Cat>());
        _mockApi.Setup(a => a.GetCatsByBreedAsync("beng")).ReturnsAsync(cats);

        _mockBreedCache.Setup(c => c.IsBreedCacheValidAsync()).ReturnsAsync(false);
        _mockBreedCache.Setup(c => c.GetCachedBreedsAsync()).ReturnsAsync(new List<Breed>());
        var sut = CreateSut();

        // Act
        await sut.SelectedBreedAsync("beng");

        // Assert
        sut.HasNoKittens.Should().BeFalse();
    }

    [Fact]
    public async Task SelectedBreedAsync_WhenCatsHaveEmptyOrWhitespaceUrls_ShouldSetHasNoKittensTrue()
    {
        // Arrange
        _mockConnectivity.Setup(c => c.IsConnected).Returns(true);
        var cats = new List<Cat>
        {
            new Cat { Id = "cat1", Url = "" },
            new Cat { Id = "cat2", Url = "   " }
        };
        _mockCatCache.Setup(c => c.GetCachedCatsByBreedAsync("beng", 10)).ReturnsAsync(new List<Cat>());
        _mockApi.Setup(a => a.GetCatsByBreedAsync("beng")).ReturnsAsync(cats);

        _mockBreedCache.Setup(c => c.IsBreedCacheValidAsync()).ReturnsAsync(false);
        _mockBreedCache.Setup(c => c.GetCachedBreedsAsync()).ReturnsAsync(new List<Breed>());
        var sut = CreateSut();

        // Act
        await sut.SelectedBreedAsync("beng");

        // Assert
        sut.HasNoKittens.Should().BeTrue();
    }

    [Fact]
    public async Task InitializeDataAsync_SelectedBreed_ShouldPreserveCoreRatingTraits()
    {
        // Arrange
        var testBreed = new Breed
        {
            Id = "beng",
            Name = "Bengal",
            Affection_level = 5,
            Adaptability = 4,
            Child_friendly = 3,
            Dog_friendly = 2,
            Energy_level = 5,
            Intelligence = 4
        };
        _mockBreedCache.Setup(c => c.IsBreedCacheValidAsync()).ReturnsAsync(true);
        _mockBreedCache.Setup(c => c.GetCachedBreedsAsync()).ReturnsAsync(new List<Breed> { testBreed });
        _mockCatCache.Setup(c => c.GetCachedCatsByBreedAsync("beng", 10)).ReturnsAsync(new List<Cat>());

        var sut = CreateSut();

        // Act
        await sut.InitializeDataAsync();

        // Assert
        sut.SelectedBreed.Should().NotBeNull();
        sut.SelectedBreed!.Affection_level.Should().Be(5);
        sut.SelectedBreed!.Adaptability.Should().Be(4);
        sut.SelectedBreed!.Child_friendly.Should().Be(3);
        sut.SelectedBreed!.Dog_friendly.Should().Be(2);
        sut.SelectedBreed!.Energy_level.Should().Be(5);
        sut.SelectedBreed!.Intelligence.Should().Be(4);
    }
}
