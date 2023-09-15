namespace Meow.ViewModels;

public partial class VoteViewModel : BaseViewModel
{
    [ObservableProperty]
    LayoutState layoutState = LayoutState.None;

    [ObservableProperty]
    string imageHeart = "icon_heart_outline.png";

    private readonly ICatService _catService;

    public VoteViewModel(ICatService catService)
    {
        Title = "Vote";

        _catService = catService;

        InitializeDataAsync();
    }

    [ObservableProperty]
    List<Cat> cats;

    public async Task InitializeDataAsync()
    {
        ImageHeart = "icon_heart_outline.png";
        LayoutState = LayoutState.None;

        Cats = await _catService.GetRandomKitty();
    }

    [RelayCommand]
    public async Task GetKittyAsync()
    {
        await InitializeDataAsync();
    }

    [RelayCommand]
    public async Task ManageFavoriteKittenAsync()
    {
        if (LayoutState == LayoutState.None)
        {
            IsBusy = true;
            await _catService.AddFavoriteKitten(Cats.FirstOrDefault().Id);
            ImageHeart = "icon_heart_solid.png";
            LayoutState = LayoutState.Success;
            IsBusy = false;
        }
        else
        {
            IsBusy = true;
            await _catService.RemoveFavoriteKitten(Cats.FirstOrDefault().Id);
            ImageHeart = "icon_heart_outline.png";
            LayoutState = LayoutState.None;
            IsBusy = false;
        }
    }
}
