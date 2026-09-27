namespace Meow.Presentation.ViewModels;

/// <summary>
/// ViewModel for the cat voting screen with offline support
/// </summary>
public partial class VoteViewModel : BaseViewModel
{
    private readonly GetVotingCatsUseCase _getVotingCatsUseCase;
    private readonly ManageFavoritesUseCase _manageFavoritesUseCase;
    private readonly IConnectivityProvider _connectivityProvider;

    private int _currentCatIndex;
    private readonly HashSet<string> _seenCatIds = new();
    private readonly Queue<string> _recentSeenCatQueue = new();
    private const int MaxSeenCatTracking = 200;
    private bool _isPrefetching;
    private const int PrefetchThreshold = 4;

    private const int PlaceholderMinimumMs = 500;

    [ObservableProperty]
    private bool isTransitioning;

    /// <summary>
    /// Tracks whether the API was previously failing so we can show recovery messages
    /// </summary>
    private bool _wasOfflineOrApiError;

    [ObservableProperty]
    private LayoutState layoutState = LayoutState.None;

    [ObservableProperty]
    private string imageHeart = "icon_heart_outline.png";

    [ObservableProperty]
    private bool isAnimation;

    [ObservableProperty]
    private bool isHidden = true;

    [ObservableProperty]
    private List<Cat>? cats;

    [ObservableProperty]
    private Cat? currentCat;

    /// <summary>
    /// True when a fatal error occurred (no data at all) — shows full error state view
    /// </summary>
    [ObservableProperty]
    private bool hasError;

    /// <summary>
    /// Warning banner text shown at the top (connectivity/API status)
    /// </summary>
    [ObservableProperty]
    private string? warningMessage;

    /// <summary>
    /// Success/recovery banner text shown at the top
    /// </summary>
    [ObservableProperty]
    private string? successMessage;

    /// <summary>
    /// True when offline and no cached data available
    /// </summary>
    [ObservableProperty]
    private bool isNoInternet;

    /// <summary>
    /// True when API returned an error but we have no fallback data
    /// </summary>
    [ObservableProperty]
    private bool isApiError;

    /// <summary>
    /// Event raised when the ViewModel wants the View to fade-out the card before loading new data.
    /// </summary>
    public event Func<Task>? TransitionRequested;

    /// <summary>
    /// Event raised after new data has loaded so the View can fade the card back in.
    /// </summary>
    public event Func<Task>? TransitionCompleted;

    /// <summary>
    /// Raised after the new image URL is set but before the card fades in.
    /// The View should preload / warm the image cache so there is no flash.
    /// </summary>
    public event Func<string?, Task>? PreloadImageRequested;

    /// <summary>
    /// Event raised when the View should trigger a native red hearts confetti animation.
    /// </summary>
    public event Func<Task>? HeartAnimationRequested;

    public VoteViewModel(
        GetVotingCatsUseCase getVotingCatsUseCase,
        ManageFavoritesUseCase manageFavoritesUseCase,
        IConnectivityProvider connectivityProvider)
    {
        Title = "Vote";
        _getVotingCatsUseCase = getVotingCatsUseCase;
        _manageFavoritesUseCase = manageFavoritesUseCase;
        _connectivityProvider = connectivityProvider;

        _connectivityProvider.ConnectivityChanged += OnConnectivityChanged;
    }

    private void OnConnectivityChanged(object? sender, bool isConnected)
    {
        if (isConnected)
        {
            // Connection restored
            WarningMessage = null;
            IsOffline = false;

            if (_wasOfflineOrApiError)
            {
                _wasOfflineOrApiError = false;
                ShowSuccessNotification("You're back online!");

                // Auto-retry if we were in an error state
                if (HasError)
                {
                    _ = RetryAsync();
                }
            }
        }
        else
        {
            // Lost connection
            IsOffline = true;
            _wasOfflineOrApiError = true;
            SuccessMessage = null;
            WarningMessage = "No internet connection";
        }
    }

    private async void ShowSuccessNotification(string message)
    {
        SuccessMessage = message;
        await Task.Delay(4000);
        SuccessMessage = null;
    }

    private async void ShowWarningNotification(string message, int delayMs = 5000)
    {
        WarningMessage = message;
        await Task.Delay(delayMs);
        if (WarningMessage == message)
            WarningMessage = null;
    }

    /// <summary>
    /// Loads a random cat with offline fallback
    /// </summary>
    public async Task InitializeDataAsync(bool forceRefresh = false, bool keepHiddenUntilCallerShows = false)
    {
        ImageHeart = "icon_heart_outline.png";
        LayoutState = LayoutState.None;
        HasError = false;
        IsNoInternet = false;
        IsApiError = false;
        ClearStatus();

        IsHidden = true;

        var result = await _getVotingCatsUseCase.ExecuteAsync(forceRefresh);

        Cats = result.Data?.ToList();
        IsOffline = result.Source == ResultSource.Cache && !string.IsNullOrEmpty(result.ErrorMessage);

        if (!result.IsSuccess && (Cats == null || Cats.Count == 0))
        {
            // Fatal error — no data to show
            HasError = true;
            _wasOfflineOrApiError = true;
            CurrentCat = null;
            var errorMsg = result.ErrorMessage ?? "";
            IsNoInternet = errorMsg.Contains("offline", StringComparison.OrdinalIgnoreCase)
                        || errorMsg.Contains("internet", StringComparison.OrdinalIgnoreCase)
                        || errorMsg.Contains("connection", StringComparison.OrdinalIgnoreCase)
                        || errorMsg.Contains("network", StringComparison.OrdinalIgnoreCase);
            IsApiError = !IsNoInternet;

            WarningMessage = IsNoInternet
                ? "No internet connection"
                : "The Cat API is not responding";
            IsHidden = false;
            return;
        }

        if (result.Source == ResultSource.Cache && result.ErrorMessage != null)
        {
            // Partial offline — we have cached data but API failed
            _wasOfflineOrApiError = true;
            ShowWarningNotification(result.ErrorMessage);
        }

        if (!forceRefresh && Cats?.Count > 2)
        {
            // Shuffle to avoid always showing the same first item when data comes from cache.
            Cats = Cats.OrderBy(_ => Guid.NewGuid()).ToList();
        }

        _currentCatIndex = 0;
        CurrentCat = Cats?.FirstOrDefault();
        if (CurrentCat != null)
        {
            RecordSeenCat(CurrentCat.Id);
        }
        await UpdateFavoriteStateForCurrentCatAsync();

        if (!keepHiddenUntilCallerShows)
        {
            IsHidden = false;
        }

        // Proactively prefetch upcoming cats if online and buffer is low
        if (_connectivityProvider.IsConnected && Cats != null && Cats.Count - (_currentCatIndex + 1) <= PrefetchThreshold)
        {
            _ = PrefetchFreshCatsAsync();
        }
    }

    private async Task UpdateFavoriteStateForCurrentCatAsync()
    {
        ImageHeart = "icon_heart_outline.png";
        LayoutState = LayoutState.None;

        if (CurrentCat == null)
            return;

        var isFavorite = await _manageFavoritesUseCase.IsFavoriteAsync(CurrentCat.Id);
        if (isFavorite)
        {
            ImageHeart = "icon_heart_solid.png";
            LayoutState = LayoutState.Success;
        }
    }

    private void RecordSeenCat(string? id)
    {
        if (string.IsNullOrEmpty(id)) return;
        if (_seenCatIds.Add(id))
        {
            _recentSeenCatQueue.Enqueue(id);
            if (_recentSeenCatQueue.Count > MaxSeenCatTracking)
            {
                var oldest = _recentSeenCatQueue.Dequeue();
                _seenCatIds.Remove(oldest);
            }
        }
    }

    private async Task PrefetchFreshCatsAsync()
    {
        if (_isPrefetching || !_connectivityProvider.IsConnected)
            return;

        _isPrefetching = true;
        try
        {
            var result = await _getVotingCatsUseCase.ExecuteAsync(forceRefresh: true);
            if (result.IsSuccess && result.Data?.Count > 0)
            {
                var existingIds = new HashSet<string>(Cats?.Select(c => c.Id) ?? Enumerable.Empty<string>());
                // Prefer cats that haven't been shown in this session and aren't already in buffer
                var freshCats = result.Data
                    .Where(c => !existingIds.Contains(c.Id) && !_seenCatIds.Contains(c.Id))
                    .ToList();

                if (freshCats.Count == 0)
                {
                    freshCats = result.Data.Where(c => !existingIds.Contains(c.Id)).ToList();
                }

                if (freshCats.Count > 0 && Cats != null)
                {
                    Cats.AddRange(freshCats);
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"PrefetchFreshCatsAsync error: {ex.Message}");
        }
        finally
        {
            _isPrefetching = false;
        }
    }

    private async Task MoveToNextCatOrReloadAsync(bool forceRefresh, string? previousCatId = null)
    {
        previousCatId ??= CurrentCat?.Id;

        // Advance in current buffer if cats remain
        if (!forceRefresh && Cats != null && _currentCatIndex + 1 < Cats.Count)
        {
            _currentCatIndex++;
            CurrentCat = Cats[_currentCatIndex];
            if (CurrentCat != null)
            {
                RecordSeenCat(CurrentCat.Id);
            }
            await UpdateFavoriteStateForCurrentCatAsync();

            if (Cats.Count - (_currentCatIndex + 1) <= PrefetchThreshold)
            {
                _ = PrefetchFreshCatsAsync();
            }
            return;
        }

        // Buffer exhausted or forceRefresh: fetch fresh batch from API
        var result = await _getVotingCatsUseCase.ExecuteAsync(forceRefresh: true);
        if (result.Data?.Count > 0)
        {
            var newCats = result.Data.ToList();
            var unseenCats = newCats.Where(c => c.Id != previousCatId && !_seenCatIds.Contains(c.Id)).ToList();
            var availableCats = unseenCats.Count > 0
                ? unseenCats
                : newCats.Where(c => c.Id != previousCatId).ToList();

            Cats = availableCats.Count > 0 ? availableCats : newCats;

            _currentCatIndex = 0;
            CurrentCat = Cats.FirstOrDefault();
            if (CurrentCat != null)
            {
                RecordSeenCat(CurrentCat.Id);
            }
            IsOffline = result.Source == ResultSource.Cache && !string.IsNullOrEmpty(result.ErrorMessage);
            await UpdateFavoriteStateForCurrentCatAsync();

            _ = PrefetchFreshCatsAsync();
            return;
        }

        // If fetch returned no data (offline/error), fallback to cycling through existing cats
        if (Cats != null && Cats.Count > 0)
        {
            _currentCatIndex = (_currentCatIndex + 1) % Cats.Count;
            CurrentCat = Cats[_currentCatIndex];
            await UpdateFavoriteStateForCurrentCatAsync();
        }
    }

    private async Task SafeInvokeAsync(Func<Task>? handler)
    {
        if (handler == null) return;

        try
        {
            await handler.Invoke();
        }
        catch
        {
            // Ignore animation failures (e.g., page navigated away) to avoid locking UI state.
        }
    }

    [RelayCommand]
    public async Task GetKittyAsync()
    {
        // Drop taps while a transition is already running.
        if (IsTransitioning)
            return;

        IsTransitioning = true;

        try
        {
            await AdvanceToNextCatInternalAsync();
        }
        finally
        {
            IsTransitioning = false;
        }
    }

    /// <summary>
    /// Triggered when the user votes 'LOVE IT': seamlessly advances to the next random cat
    /// without adding to favorites or triggering heart animation.
    /// Favoriting is exclusively handled by tapping the cat image.
    /// </summary>
    [RelayCommand]
    public async Task LoveKittyAsync()
    {
        if (IsTransitioning || IsHidden || CurrentCat == null)
            return;

        IsTransitioning = true;

        try
        {
            await AdvanceToNextCatInternalAsync();
        }
        finally
        {
            IsTransitioning = false;
        }
    }

    private async Task AdvanceToNextCatInternalAsync()
    {
        IsAnimation = false;

        // Fade out card
        await SafeInvokeAsync(TransitionRequested);

        var previousCatId = CurrentCat?.Id;

        // Clear the current image source so old bitmap cannot flash back
        CurrentCat = null;

        // Show placeholder (paw) and keep it for a minimum duration
        IsHidden = true;
        var placeholderDelay = Task.Delay(PlaceholderMinimumMs);

        // Move to next cat (or reload if end of list reached)
        await MoveToNextCatOrReloadAsync(forceRefresh: false, previousCatId);

        // Preload the new image into the cache so it renders instantly when revealed
        if (PreloadImageRequested != null && CurrentCat?.Url != null)
        {
            await SafeInvokeAsync(() => PreloadImageRequested.Invoke(CurrentCat.Url));
        }

        // Ensure the placeholder is visible long enough
        await placeholderDelay;

        if (CurrentCat == null)
        {
            HasError = true;
            IsHidden = false;
            return;
        }

        // Reveal the image source, then fade the card in
        IsHidden = false;
        await SafeInvokeAsync(TransitionCompleted);
    }

    /// <summary>
    /// Retry loading after an error state
    /// </summary>
    [RelayCommand]
    public async Task RetryAsync()
    {
        HasError = false;
        IsNoInternet = false;
        IsApiError = false;
        IsHidden = true;
        WarningMessage = null;
        ClearStatus();

        if (IsTransitioning)
            return;

        IsTransitioning = true;

        try
        {
            var placeholderDelay = Task.Delay(PlaceholderMinimumMs);
            await InitializeDataAsync(forceRefresh: true, keepHiddenUntilCallerShows: true);

            if (PreloadImageRequested != null && CurrentCat?.Url != null)
            {
                await SafeInvokeAsync(() => PreloadImageRequested.Invoke(CurrentCat.Url));
            }

            await placeholderDelay;

            IsHidden = false;
            await SafeInvokeAsync(TransitionCompleted);
        }
        finally
        {
            IsTransitioning = false;
        }
    }

    [RelayCommand]
    public async Task ManageFavoriteKittenAsync()
    {
        // Block favorite toggle during transition
        if (IsTransitioning) return;

        IsBusy = true;

        bool isAdding = LayoutState == LayoutState.None;
        await ToggleFavoriteAsync(isAdding, triggerAnimation: true);

        IsBusy = false;
    }

    public async Task ToggleFavoriteAsync(bool isAdding, bool triggerAnimation = true)
    {
        var currentCat = CurrentCat;
        if (currentCat == null) return;

        if (isAdding)
        {
            var result = await _manageFavoritesUseCase.AddFavoriteAsync(currentCat);
            if (result.IsSuccess)
            {
                ImageHeart = "icon_heart_solid.png";
                LayoutState = LayoutState.Success;
                Progress = TimeSpan.Zero;
                IsAnimation = true;
                if (triggerAnimation)
                {
                    _ = SafeInvokeAsync(HeartAnimationRequested);
                }

                if (result.Source == ResultSource.Cache)
                {
                    ShowWarningNotification("Added to favorites (will sync when online)", 3000);
                }
            }
            else
            {
                ShowWarningNotification(result.ErrorMessage ?? "Failed to add favorite.", 3000);
            }
        }
        else
        {
            IsAnimation = false;
            var result = await _manageFavoritesUseCase.RemoveFavoriteAsync(currentCat.Id);
            if (result.IsSuccess)
            {
                ImageHeart = "icon_heart_outline.png";
                LayoutState = LayoutState.None;
            }
            else
            {
                ShowWarningNotification(result.ErrorMessage ?? "Failed to remove favorite.", 3000);
            }
        }
    }
}
