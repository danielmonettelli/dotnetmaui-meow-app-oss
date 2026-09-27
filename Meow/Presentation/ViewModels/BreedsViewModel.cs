namespace Meow.Presentation.ViewModels;

/// <summary>
/// ViewModel for the cat breeds screen with offline support
/// </summary>
public partial class BreedsViewModel : BaseViewModel
{
    private readonly GetBreedsUseCase _getBreedsUseCase;
    private readonly GetCatsByBreedUseCase _getCatsByBreedUseCase;

    [ObservableProperty]
    private List<Breed>? breeds;

    [ObservableProperty]
    private Breed? selectedBreed;

    [ObservableProperty]
    private List<Cat>? kittensByBreed;

    [ObservableProperty]
    private bool hasNoKittens;

    [ObservableProperty]
    private bool isLoadBreeds;

    private bool _isInitializing;

    partial void OnSelectedBreedChanged(Breed? value)
    {
        if (value != null && !_isInitializing)
        {
            _ = SelectedBreedAsync(value.Id);
        }
    }

    public BreedsViewModel(
        GetBreedsUseCase getBreedsUseCase,
        GetCatsByBreedUseCase getCatsByBreedUseCase)
    {
        Title = "Breeds";
        _getBreedsUseCase = getBreedsUseCase;
        _getCatsByBreedUseCase = getCatsByBreedUseCase;
    }

    /// <summary>
    /// Loads breeds with offline support
    /// </summary>
    public async Task InitializeDataAsync()
    {
        IsBusy = true;
        _isInitializing = true;
        ClearStatus();

        try
        {
            var breedsResult = await _getBreedsUseCase.ExecuteAsync();

            Breeds = breedsResult.Data;
            IsOffline = breedsResult.Source == ResultSource.Cache && !string.IsNullOrEmpty(breedsResult.ErrorMessage);

            if (!breedsResult.IsSuccess)
            {
                StatusMessage = breedsResult.ErrorMessage ?? "Unable to load breeds.";
            }
            else if (breedsResult.Source == ResultSource.Cache && breedsResult.ErrorMessage != null)
            {
                _ = ShowTemporaryStatusAsync(breedsResult.ErrorMessage);
            }

            SelectedBreed = Breeds?.FirstOrDefault();

            if (SelectedBreed != null)
            {
                await SelectedBreedAsync(SelectedBreed.Id);
            }
            else
            {
                HasNoKittens = true;
            }
        }
        finally
        {
            _isInitializing = false;
            IsBusy = false;
        }
    }

    /// <summary>
    /// Loads cats for a specific breed
    /// </summary>
    public async Task SelectedBreedAsync(string id)
    {
        if (string.IsNullOrEmpty(id)) return;

        IsLoadBreeds = true;
        ClearStatus();

        var result = await _getCatsByBreedUseCase.ExecuteAsync(id);
        var validCats = result.Data?.Where(c => !string.IsNullOrWhiteSpace(c.Url)).ToList();
        KittensByBreed = validCats;
        HasNoKittens = KittensByBreed == null || KittensByBreed.Count == 0;

        if (!result.IsSuccess && result.ErrorMessage != null)
        {
            _ = ShowTemporaryStatusAsync(result.ErrorMessage);
        }

        IsLoadBreeds = false;
    }
}
