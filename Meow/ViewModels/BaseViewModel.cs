namespace Meow.ViewModels;

/// <summary>
/// Base class for all ViewModels providing common functionality
/// </summary>
public partial class BaseViewModel : ObservableObject
{
    #region Observable Properties

    /// <summary>
    /// Title displayed in the UI for the current view
    /// </summary>
    [ObservableProperty]
    private string title;

    /// <summary>
    /// Indicates if the ViewModel is currently busy processing
    /// </summary>
    [ObservableProperty]
    private bool isBusy;

    /// <summary>
    /// Progress tracking for animations or operations
    /// </summary>
    [ObservableProperty]
    private TimeSpan progress;

    #endregion

    #region Commands

    /// <summary>
    /// Command to toggle between light and dark theme
    /// </summary>
    [RelayCommand]
    public void SelectTheme()
    {
        // Get current theme
        AppTheme currentTheme = Application.Current.RequestedTheme;
        
        // Toggle to opposite theme
        AppTheme newTheme = currentTheme == AppTheme.Dark ? AppTheme.Light : AppTheme.Dark;
        
        // Apply the new theme
        Application.Current.UserAppTheme = newTheme;
    }

    #endregion
}
