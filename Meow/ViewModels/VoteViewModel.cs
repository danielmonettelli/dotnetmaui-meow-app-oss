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

    public override async Task InitializeDataAsync()
    {
        await base.InitializeDataAsync();

        ImageHeart = "icon_heart_outline.png";
        LayoutState = LayoutState.None;

        Cats = await _catService.GetRandomKitty();
    }

    [RelayCommand]
    public async Task GetKittyAsync()
    {
        ImageHeart = "icon_heart_outline.png";

        Cats = await _catService.GetRandomKitty();
    }

    [RelayCommand]
    public async Task ManageFavoriteKittenAsync()
    {
        switch (LayoutState)
        {
            case LayoutState.None:
                ImageHeart = "icon_heart_solid.png";
                LayoutState = LayoutState.Success;
                await _catService.AddFavoriteKitten(Cats.FirstOrDefault().Id);
                break;

            default:
                ImageHeart = "icon_heart_outline.png";
                LayoutState = LayoutState.None;
                await _catService.RemoveFavoriteKitten(Cats.FirstOrDefault().Id);
                break;
        }
    }
}
