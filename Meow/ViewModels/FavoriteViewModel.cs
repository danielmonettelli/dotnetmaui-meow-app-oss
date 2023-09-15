namespace Meow.ViewModels;

public partial class FavoriteViewModel : BaseViewModel
{
    [ObservableProperty]
    List<FavoriteCatResponse> favoriteCats = new();

    [ObservableProperty]
    FavoriteCatResponse selectedFavoriteCat = new();

    private readonly ICatService _catService;

    public FavoriteViewModel(ICatService catService)
    {
        Title = "Favorites";

        _catService = catService;

        InitializeDataAsync();
    }

    public async override Task InitializeDataAsync()
    {
        IsBusy = true;

        await base.InitializeDataAsync();

        FavoriteCats = await _catService.GetFavoriteKittens();

        IsBusy = false;
    }

    [RelayCommand]
    public async Task DeleteFavoriteKittenAsync()
    {
        await _catService
              .DeleteFavoriteKitten(SelectedFavoriteCat.Id);

        await InitializeDataAsync();
    }
}
