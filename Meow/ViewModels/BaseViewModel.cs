namespace Meow.ViewModels;

public partial class BaseViewModel : ObservableObject
{
    public virtual Task InitializeDataAsync()
    {
        return Task.CompletedTask;
    }

    [ObservableProperty]
    private string title;

    [ObservableProperty]
    private bool isBusy;
}
