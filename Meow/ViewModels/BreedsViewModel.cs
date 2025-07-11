namespace Meow.ViewModels;

/// <summary>
/// ViewModel for the cat breeds screen
/// </summary>
public partial class BreedsViewModel : BaseViewModel
{
    #region Private Fields

    private readonly ICacheService _cacheService;

    #endregion

    #region Observable Properties

    /// <summary>
    /// List of all available cat breeds
    /// </summary>
    [ObservableProperty]
    private List<Breed> breeds;

    /// <summary>
    /// Currently selected breed
    /// </summary>
    [ObservableProperty]
    private Breed selectedBreed;

    /// <summary>
    /// Collection of kittens filtered by selected breed
    /// </summary>
    [ObservableProperty]
    private List<Cat> kittensByBreed;

    /// <summary>
    /// Indicates if breed content is currently loading
    /// </summary>
    [ObservableProperty]
    private bool isLoadBreeds;

    #endregion

    #region Property Changed Methods

    /// <summary>
    /// Automatically triggered when SelectedBreed property changes
    /// </summary>
    /// <param name="value">The new selected breed</param>
    partial void OnSelectedBreedChanged(Breed value)
    {
        // Load kittens for the newly selected breed
        _ = SelectedBreedAsync(value.Id);
    }

    #endregion

    #region Constructor

    public BreedsViewModel(ICacheService cacheService)
    {
        Title = "Breeds";
        _cacheService = cacheService;

        // Initialize data loading when instance is created
        _ = InitializeDataAsync();
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Initializes the ViewModel data by loading breeds and kittens
    /// </summary>
    public async Task InitializeDataAsync()
    {
        IsBusy = true;

        // Load all available breeds from cache (with fallback to API)
        Breeds = await _cacheService.GetBreedsAsync();

        // Select the first breed by default
        SelectedBreed = Breeds?.FirstOrDefault();

        // Load kittens for the selected breed if available
        if (SelectedBreed != null)
        {
            KittensByBreed = await _cacheService.GetCatsByBreedAsync(SelectedBreed.Id);
        }

        IsBusy = false;
    }

    /// <summary>
    /// Loads kittens for a specific breed
    /// </summary>
    /// <param name="id">The breed ID to filter kittens</param>
    public async Task SelectedBreedAsync(string id)
    {
        if (string.IsNullOrEmpty(id)) return;

        IsLoadBreeds = true;

        // Get kittens that match the selected breed from cache
        KittensByBreed = await _cacheService.GetCatsByBreedAsync(id);

        IsLoadBreeds = false;
    }

    #endregion
}
