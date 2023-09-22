namespace Meow.ViewModels;

public partial class BaseViewModel : ObservableObject
{
    [ObservableProperty]
    private string title;

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private TimeSpan progress;

    [RelayCommand]
    public void SelectTheme()
    {
        AppTheme currentTheme = Application.Current.RequestedTheme;
        AppTheme newTheme = currentTheme == AppTheme.Dark ? AppTheme.Light : AppTheme.Dark;
        Application.Current.UserAppTheme = newTheme;
    }
}
