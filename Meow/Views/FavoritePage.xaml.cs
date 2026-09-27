namespace Meow.Views;

public partial class FavoritePage : ContentPage
{
    private readonly FavoriteViewModel vm;

    public FavoritePage(FavoriteViewModel favoriteViewModel)
    {
        InitializeComponent();
        vm = favoriteViewModel;

        BindingContext = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await vm.InitializeDataAsync();
    }

    private void ContentPage_SizeChanged(object? sender, EventArgs e)
    {
        if (Width > 0)
        {
            vm.Columns = Math.Max(1, (int)(Width / 174));
        }
    }
}