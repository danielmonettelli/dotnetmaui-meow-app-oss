namespace Meow.Presentation.ViewModels;

/// <summary>
/// Base class for all ViewModels providing common functionality
/// </summary>
public partial class BaseViewModel : ObservableObject
{
    /// <summary>
    /// Title displayed in the UI
    /// </summary>
    [ObservableProperty]
    private string title = string.Empty;

    /// <summary>
    /// Indicates if the ViewModel is currently busy
    /// </summary>
    [ObservableProperty]
    private bool isBusy;

    /// <summary>
    /// Progress tracking for animations
    /// </summary>
    [ObservableProperty]
    private TimeSpan progress;

    /// <summary>
    /// Indicates whether device is offline
    /// </summary>
    [ObservableProperty]
    private bool isOffline;

    /// <summary>
    /// User-facing error or info message
    /// </summary>
    [ObservableProperty]
    private string? statusMessage;

    /// <summary>
    /// Toggle between light and dark theme
    /// </summary>
    [RelayCommand]
    public void SelectTheme()
    {
        var currentTheme = Application.Current?.RequestedTheme ?? AppTheme.Light;
        var newTheme = currentTheme == AppTheme.Dark ? AppTheme.Light : AppTheme.Dark;

        if (Application.Current != null)
            Application.Current.UserAppTheme = newTheme;
    }

    /// <summary>
    /// Clears the status message
    /// </summary>
    protected void ClearStatus() => StatusMessage = null;

    /// <summary>
    /// Sets an info status message that auto-clears after a delay
    /// </summary>
    protected async Task ShowTemporaryStatusAsync(string message, int delayMs = 3000)
    {
        StatusMessage = message;
        await Task.Delay(delayMs);
        ClearStatus();
    }
}
