namespace Meow.ViewModels;

/// <summary>
/// ViewModel for the favorite cats screen
/// </summary>
public partial class FavoriteViewModel : BaseViewModel
{
    #region Private Fields

    private readonly ICatService _catService;

    #endregion

    #region Observable Properties

    /// <summary>
    /// Collection of cats marked as favorites
    /// </summary>
    [ObservableProperty]
    private List<FavoriteCatResponse> favoriteCats = new();

    /// <summary>
    /// Currently selected favorite cat
    /// </summary>
    [ObservableProperty]
    private FavoriteCatResponse selectedFavoriteCat = new();

    /// <summary>
    /// Number of columns for the grid display
    /// </summary>
    [ObservableProperty]
    private int columns;

    #endregion

    #region Constructor

    public FavoriteViewModel(ICatService catService)
    {
        Title = "Favorites";
        _catService = catService;

        // Initialize data loading when instance is created
        _ = InitializeDataAsync();
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Initializes the ViewModel data by loading favorite cats
    /// </summary>
    public async Task InitializeDataAsync()
    {
        await PerformOperationAsync(async () =>
        {
            // Load all favorite cats
            FavoriteCats = await _catService.GetFavoriteKittens();
        });
    }

    #endregion

    #region Commands

    /// <summary>
    /// Command to remove a cat from favorites
    /// </summary>
    [RelayCommand]
    public async Task DeleteFavoriteKittenAsync()
    {
        await PerformOperationAsync(async () =>
        {
            // Remove selected cat from favorites
            await _catService.DeleteFavoriteKitten(SelectedFavoriteCat.Id);
            
            // Refresh the favorites list
            FavoriteCats = await _catService.GetFavoriteKittens();
        });
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Helper method to handle operations with busy state management
    /// </summary>
    /// <param name="operation">The operation to perform</param>
    private async Task PerformOperationAsync(Func<Task> operation)
    {
        // Set busy state to show loading indicator
        IsBusy = true;

        // Execute the provided operation
        await operation.Invoke();

        // Reset busy state
        IsBusy = false;
    }

    #endregion
}
