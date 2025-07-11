namespace Meow.ViewModels;

/// <summary>
/// ViewModel for the favorite cats screen
/// </summary>
public partial class FavoriteViewModel : BaseViewModel
{
    #region Private Fields

    private readonly ICacheService _cacheService;

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

    /// <summary>
    /// Number of unsynced favorites
    /// </summary>
    [ObservableProperty]
    private int unsyncedCount;

    /// <summary>
    /// Indicates if sync is in progress
    /// </summary>
    [ObservableProperty]
    private bool isSyncing;

    #endregion

    #region Constructor

    public FavoriteViewModel(ICacheService cacheService)
    {
        Title = "Favorites";
        _cacheService = cacheService;

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
            // Load all favorite cats from cache (with sync if online)
            FavoriteCats = await _cacheService.GetFavoritesAsync();
            
            // Update unsynced count
            UnsyncedCount = await _cacheService.GetUnsyncedFavoritesCountAsync();
        });
    }

    /// <summary>
    /// Manually syncs favorites with server
    /// </summary>
    public async Task SyncFavoritesAsync()
    {
        IsSyncing = true;
        
        var success = await _cacheService.SyncFavoritesAsync();
        
        if (success)
        {
            // Refresh data after sync
            await InitializeDataAsync();
        }
        
        IsSyncing = false;
    }

    #endregion

    #region Commands

    /// <summary>
    /// Command to remove a cat from favorites
    /// </summary>
    [RelayCommand]
    public async Task DeleteFavoriteKittenAsync()
    {
        if (SelectedFavoriteCat?.Image?.Id == null) return;

        await PerformOperationAsync(async () =>
        {
            // Remove selected cat from favorites using cache service
            var success = await _cacheService.RemoveFavoriteAsync(SelectedFavoriteCat.Image.Id);
            
            if (success)
            {
                // Refresh the favorites list
                FavoriteCats = await _cacheService.GetFavoritesAsync();
                
                // Update unsynced count
                UnsyncedCount = await _cacheService.GetUnsyncedFavoritesCountAsync();
            }
        });
    }

    /// <summary>
    /// Command to manually sync favorites
    /// </summary>
    [RelayCommand]
    public async Task SyncFavoritesCommand()
    {
        await SyncFavoritesAsync();
    }

    /// <summary>
    /// Command to refresh favorites
    /// </summary>
    [RelayCommand]
    public async Task RefreshFavoritesAsync()
    {
        await InitializeDataAsync();
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
