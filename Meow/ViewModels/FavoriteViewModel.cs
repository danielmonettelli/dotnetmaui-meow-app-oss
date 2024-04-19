namespace Meow.ViewModels;

public partial class FavoriteViewModel : BaseViewModel
{
    [ObservableProperty]
    private List<FavoriteCatResponse> favoriteCats = new();

    [ObservableProperty]
    private FavoriteCatResponse selectedFavoriteCat = new();

    [ObservableProperty]
    private int columns;

    private readonly ICatService _catService;

    public FavoriteViewModel(ICatService catService)
    {
        Title = "Favorites";

        _catService = catService;

        InitializeDataAsync();
    }

    public async Task InitializeDataAsync()
    {
        await PerformOperationAsync(async () =>
        {
            FavoriteCats = await _catService.GetFavoriteKittens();
        });
    }

    [RelayCommand]
    public async Task DeleteFavoriteKittenAsync()
    {
        await PerformOperationAsync(async () =>
        {
            await _catService.DeleteFavoriteKitten(SelectedFavoriteCat.Id);
            FavoriteCats = await _catService.GetFavoriteKittens();
        });
    }

    private async Task PerformOperationAsync(Func<Task> operation)
    {
        IsBusy = true;

        await operation.Invoke();

        IsBusy = false;
    }
}
