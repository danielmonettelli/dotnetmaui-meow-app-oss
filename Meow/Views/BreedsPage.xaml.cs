namespace Meow.Views;

public partial class BreedsPage : ContentPage
{
    private readonly BreedsViewModel vm = new(new CatService());

    public BreedsPage()
    {
        InitializeComponent();

        BindingContext = vm;
    }

    protected async override void OnAppearing()
    {
        base.OnAppearing();

        await vm.InitializeDataAsync();
    }
}