namespace Meow.ViewModels;

/// <summary>
/// ViewModel for the cat breeds screen
/// </summary>
public partial class BreedsViewModel : BaseViewModel
{
    #region Private Fields

    private readonly ICatService _catService;

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

    public BreedsViewModel(ICatService catService)
    {
        Title = "Breeds";
        _catService = catService;

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

        // Load all available breeds
        Breeds = await _catService.GetBreeds();

        // Select the first breed by default
        SelectedBreed = Breeds.FirstOrDefault();

        // Load kittens for the selected breed
        KittensByBreed = await _catService.GetRandomKittensByBreed(SelectedBreed.Id);

        IsBusy = false;
    }

    /// <summary>
    /// Loads kittens for a specific breed
    /// </summary>
    /// <param name="id">The breed ID to filter kittens</param>
    public async Task SelectedBreedAsync(string id)
    {
        IsLoadBreeds = true;

        // Get kittens that match the selected breed
        KittensByBreed = await _catService.GetRandomKittensByBreed(id);

        IsLoadBreeds = false;
    }

    #endregion
}
