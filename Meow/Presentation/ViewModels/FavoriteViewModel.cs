namespace Meow.Presentation.ViewModels;

/// <summary>
/// ViewModel for the favorite cats screen with offline and sync support
/// </summary>
public partial class FavoriteViewModel : BaseViewModel
{
    private readonly ManageFavoritesUseCase _manageFavoritesUseCase;

    [ObservableProperty]
    private List<FavoriteCatResponse> favoriteCats = new();

    [ObservableProperty]
    private FavoriteCatResponse selectedFavoriteCat = new();

    [ObservableProperty]
    private int columns = 2;

    [ObservableProperty]
    private double itemWidth = 160;

    [ObservableProperty]
    private int unsyncedCount;

    [ObservableProperty]
    private bool isSyncing;

    /// <summary>
    /// True when favorite list is empty — shows empty state message
    /// </summary>
    public bool HasNoFavorites => FavoriteCats == null || FavoriteCats.Count == 0;

    partial void OnFavoriteCatsChanged(List<FavoriteCatResponse> value)
    {
        OnPropertyChanged(nameof(HasNoFavorites));
    }

    /// <summary>
    /// Triggered by CollectionView selection change when user selects a favorite cat to remove
    /// </summary>
    partial void OnSelectedFavoriteCatChanged(FavoriteCatResponse value)
    {
        if (value?.Image?.Id != null)
        {
            _ = DeleteFavoriteKittenAsync();
        }
    }

    public FavoriteViewModel(ManageFavoritesUseCase manageFavoritesUseCase)
    {
        Title = "Favorites";
        _manageFavoritesUseCase = manageFavoritesUseCase;
    }

    /// <summary>
    /// Loads favorites with sync status
    /// </summary>
    public async Task InitializeDataAsync()
    {
        await PerformOperationAsync(async () =>
        {
            ClearStatus();

            var result = await _manageFavoritesUseCase.GetFavoritesAsync();
            FavoriteCats = result.Data ?? new List<FavoriteCatResponse>();
            IsOffline = result.Source == ResultSource.Cache && !string.IsNullOrEmpty(result.ErrorMessage);

            UnsyncedCount = await _manageFavoritesUseCase.GetUnsyncedCountAsync();

            if (!result.IsSuccess && result.ErrorMessage != null)
            {
                StatusMessage = result.ErrorMessage;
            }
            else if (UnsyncedCount > 0)
            {
                StatusMessage = $"{UnsyncedCount} favorite(s) pending sync";
            }
        });
    }

    /// <summary>
    /// Manually syncs favorites with server
    /// </summary>
    public async Task SyncFavoritesAsync()
    {
        IsSyncing = true;

        var result = await _manageFavoritesUseCase.SyncAsync();

        if (result.IsSuccess)
        {
            await InitializeDataAsync();
            _ = ShowTemporaryStatusAsync("Sync completed successfully");
        }
        else
        {
            _ = ShowTemporaryStatusAsync(result.ErrorMessage ?? "Sync failed");
        }

        IsSyncing = false;
    }

    [RelayCommand]
    public async Task DeleteFavoriteKittenAsync()
    {
        if (SelectedFavoriteCat?.Image?.Id == null) return;

        var imageId = SelectedFavoriteCat.Image.Id;
        SelectedFavoriteCat = new();

        await PerformOperationAsync(async () =>
        {
            var result = await _manageFavoritesUseCase.RemoveFavoriteAsync(imageId);

            if (result.IsSuccess)
            {
                var favResult = await _manageFavoritesUseCase.GetFavoritesAsync();
                FavoriteCats = favResult.Data ?? new List<FavoriteCatResponse>();
                UnsyncedCount = await _manageFavoritesUseCase.GetUnsyncedCountAsync();
            }
            else
            {
                _ = ShowTemporaryStatusAsync(result.ErrorMessage ?? "Failed to remove favorite.");
            }
        });
    }

    [RelayCommand]
    public async Task SyncFavoritesCommand()
    {
        await SyncFavoritesAsync();
    }

    [RelayCommand]
    public async Task RefreshFavoritesAsync()
    {
        await InitializeDataAsync();
    }

    private async Task PerformOperationAsync(Func<Task> operation)
    {
        IsBusy = true;
        await operation.Invoke();
        IsBusy = false;
    }
}
