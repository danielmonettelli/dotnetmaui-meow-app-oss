namespace Meow.ViewModels;

public partial class BreedsViewModel : BaseViewModel
{
    [ObservableProperty]
    private List<Breed> breeds;

    [ObservableProperty]
    private Breed selectedBreed;

    [ObservableProperty]
    private List<Cat> kittensByBreed;

    [ObservableProperty]
    private bool isLoadBreeds;

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

    public async Task InitializeDataAsync()
    {
        IsBusy = true;

        Breeds = await _catService.GetBreeds();

        SelectedBreed = Breeds.FirstOrDefault();

        KittensByBreed = await _catService.GetRandomKittensByBreed(SelectedBreed.Id);

        IsBusy = false;
    }

    public async Task SelectedBreedAsync(string id)
    {
        IsLoadBreeds = true;

        KittensByBreed = await _catService.GetRandomKittensByBreed(id);

        IsLoadBreeds = false;
    }
}
