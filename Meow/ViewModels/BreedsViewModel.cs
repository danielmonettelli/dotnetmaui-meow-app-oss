namespace Meow.ViewModels;

public partial class BreedsViewModel : BaseViewModel
{
    [ObservableProperty]
    private List<Breed> breeds;

    [ObservableProperty]
    private Breed selectedBreed;

    [ObservableProperty]
    private List<Cat> kittensByBreed;

    private readonly ICatService _catService;

    partial void OnSelectedBreedChanged(Breed value)
    {
        SelectedBreedAsync(value.Id);
    }

    public BreedsViewModel(ICatService catService)
    {
        Title = "Breeds";

        _catService = catService;

        InitializeDataAsync();
    }

    public async override Task InitializeDataAsync()
    {
        await base.InitializeDataAsync();

        Breeds = await _catService.GetBreeds();

        SelectedBreed = Breeds.FirstOrDefault();

        KittensByBreed = await _catService.GetRandomKittensByBreed(SelectedBreed.Id);
    }

    public async Task SelectedBreedAsync(string id)
    {
        KittensByBreed.Clear();

        KittensByBreed = await _catService.GetRandomKittensByBreed(id);
    }
}
