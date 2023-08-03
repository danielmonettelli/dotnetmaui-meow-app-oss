namespace Meow.ViewModels;

public partial class BaseViewModel : ObservableObject
{
    public virtual Task InitializeKittyDataAsync()
    {
        return Task.CompletedTask;
    }
}
