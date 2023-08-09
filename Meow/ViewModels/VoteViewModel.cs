namespace Meow.ViewModels;

public partial class VoteViewModel : BaseViewModel
{
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

        Cats = await _catService.GetRandomKitty();
    }

    [RelayCommand]
    public async Task GetKittyAsync()
    {
        Cats = await _catService.GetRandomKitty();
    }

    [RelayCommand]
    public async Task SaveFavoriteKittyAsync() =>
        await _catService.AddFavoriteKitten(Cats.FirstOrDefault().Id);
}
