namespace Meow.Views;

public partial class BreedsPage : ContentPage
{
    private readonly BreedsViewModel vm;

    public BreedsPage(BreedsViewModel breedsViewModel)
    {
        InitializeComponent();
        vm = breedsViewModel;

        BindingContext = vm;
    }
}