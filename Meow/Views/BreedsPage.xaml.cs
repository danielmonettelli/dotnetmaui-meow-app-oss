namespace Meow.Views;

public partial class BreedsPage : ContentPage
{
    private readonly BreedsViewModel vm;
    private bool _initialized;

    public BreedsPage(BreedsViewModel breedsViewModel)
    {
        InitializeComponent();
        vm = breedsViewModel;

        BindingContext = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (!_initialized)
        {
            _initialized = true;
            await vm.InitializeDataAsync();
        }
    }
}