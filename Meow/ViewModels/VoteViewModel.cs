namespace Meow.ViewModels;

public partial class VoteViewModel : BaseViewModel
{
    private readonly ICatService _catService;

    public VoteViewModel(ICatService catService)
    {
        _catService = catService;

        InitializeKittyDataAsync();
    }

    [ObservableProperty]
    List<Cat> cats;

    public override async Task InitializeKittyDataAsync()
    {
        await base.InitializeKittyDataAsync();

        Cats = await _catService.GetRandomKitty();
    }

    [RelayCommand]
    public async Task GetKittyAsync()
    {
        Cats = await _catService.GetRandomKitty();
    }
}
