namespace Meow.Views;

public partial class FavoritePage : ContentPage
{
    public FavoritePage(FavoriteViewModel vm)
    {
        InitializeComponent();

        BindingContext = vm;
    }
}