﻿namespace Meow.ViewModels;

/// <summary>
/// ViewModel for the cat voting screen
/// </summary>
public partial class VoteViewModel : BaseViewModel
{
    #region Private Fields

    private readonly ICacheService _cacheService;

    #endregion

    #region Observable Properties

    /// <summary>
    /// Layout state indicating if the cat is in favorites (Success) or not (None)
    /// </summary>
    [ObservableProperty]
    private LayoutState layoutState = LayoutState.None;

    /// <summary>
    /// Heart icon that changes depending on favorite status
    /// </summary>
    [ObservableProperty]
    private string imageHeart = "icon_heart_outline.png";

    /// <summary>
    /// Controls if the heart animation is active
    /// </summary>
    [ObservableProperty]
    private bool isAnimation;

    /// <summary>
    /// Controls visibility during cat loading
    /// </summary>
    [ObservableProperty]
    private bool isHidden;

    /// <summary>
    /// Collection of random cats (currently only one is used)
    /// </summary>
    [ObservableProperty]
    private List<Cat> cats;

    #endregion

    #region Constructor

    public VoteViewModel(ICacheService cacheService)
    {
        Title = "Vote";
        _cacheService = cacheService;

        // Initialize data loading when instance is created
        _ = InitializeDataAsync();
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Initializes the ViewModel data by loading a random cat
    /// </summary>
    public async Task InitializeDataAsync()
    {
        // Reset visual state
        ImageHeart = "icon_heart_outline.png";
        LayoutState = LayoutState.None;

        // Hide content while loading
        IsHidden = true;
        
        // Load cats from cache service (will fallback to API if needed)
        Cats = await _cacheService.GetVotingCatsAsync();
        
        // Check if current cat is in favorites
        if (Cats?.Any() == true)
        {
            var currentCat = Cats.FirstOrDefault();
            var isFavorite = await _cacheService.IsFavoriteAsync(currentCat.Id);
            
            if (isFavorite)
            {
                ImageHeart = "icon_heart_solid.png";
                LayoutState = LayoutState.Success;
            }
        }
        
        // Small delay to improve UX
        await Task.Delay(1500);
        
        // Show content
        IsHidden = false;
    }

    #endregion

    #region Commands

    /// <summary>
    /// Command to get a new random cat
    /// </summary>
    [RelayCommand]
    public async Task GetKittyAsync()
    {
        // Stop existing animations
        IsAnimation = false;
        
        // Load a new cat
        await InitializeDataAsync();
    }

    /// <summary>
    /// Command to manage the favorite status of current cat
    /// </summary>
    [RelayCommand]
    public async Task ManageFavoriteKittenAsync()
    {
        IsBusy = true;

        // Determine if we're adding or removing from favorites
        bool isAddingFavorite = (LayoutState == LayoutState.None);

        await ToggleFavoriteKittenAsync(isAddingFavorite);

        IsBusy = false;
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Manages adding or removing a cat from favorites
    /// </summary>
    /// <param name="isAdding">True to add, False to remove from favorites</param>
    private async Task ToggleFavoriteKittenAsync(bool isAdding)
    {
        var currentCat = Cats?.FirstOrDefault();
        if (currentCat == null) return;

        if (isAdding)
        {
            // Add cat to favorites using cache service
            var success = await _cacheService.AddFavoriteAsync(currentCat);
            
            if (success)
            {
                // Update UI to show it's in favorites
                ImageHeart = "icon_heart_solid.png";
                LayoutState = LayoutState.Success;

                // Start heart animation
                Progress = TimeSpan.Zero;
                IsAnimation = true;
            }
        }
        else
        {
            // Stop animation and remove from favorites
            IsAnimation = false;
            var success = await _cacheService.RemoveFavoriteAsync(currentCat.Id);
            
            if (success)
            {
                // Update UI to show it's not in favorites
                ImageHeart = "icon_heart_outline.png";
                LayoutState = LayoutState.None;
            }
        }
    }

    #endregion
}
