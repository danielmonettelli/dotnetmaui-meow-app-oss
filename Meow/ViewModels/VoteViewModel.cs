namespace Meow.ViewModels;

public partial class VoteViewModel : BaseViewModel
{
    [ObservableProperty]
    private LayoutState layoutState = LayoutState.None;

    [ObservableProperty]
    private string imageHeart = "icon_heart_outline.png";

    [ObservableProperty]
    private bool isAnimation;

    [ObservableProperty]
    private bool isHidden;

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

        IsHidden = true;
        Cats = await _catService.GetRandomKitty();
        await Task.Delay(1500);
        IsHidden = false;
    }

    [RelayCommand]
    public async Task GetKittyAsync()
    {
        IsAnimation = false;

        await InitializeDataAsync();
    }

    [RelayCommand]
    public async Task ManageFavoriteKittenAsync()
    {
        IsBusy = true;

        bool isAddingFavorite = (LayoutState == LayoutState.None);

        await ToggleFavoriteKittenAsync(isAddingFavorite);

        IsBusy = false;
    }

    private async Task ToggleFavoriteKittenAsync(bool isAdding)
    {
        if (isAdding)
        {
            await _catService.AddFavoriteKitten(Cats.FirstOrDefault().Id);
            ImageHeart = "icon_heart_solid.png";
            LayoutState = LayoutState.Success;

            Progress = TimeSpan.Zero;
            IsAnimation = true;
        }
        else
        {
            IsAnimation = false;
            await _catService.RemoveFavoriteKitten(Cats.FirstOrDefault().Id);
            ImageHeart = "icon_heart_outline.png";
            LayoutState = LayoutState.None;
        }
    }
}
