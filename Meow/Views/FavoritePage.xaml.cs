namespace Meow.Views;

public partial class FavoritePage : ContentPage
{
    private readonly FavoriteViewModel vm = new(new CatService());

    public FavoritePage()
    {
        InitializeComponent();

        BindingContext = vm;
    }

    protected async override void OnAppearing()
    {
        base.OnAppearing();

        await vm.InitializeDataAsync();
    }

    private void ContentPage_SizeChanged(object sender, EventArgs e)
    {
        vm.Columns = (int)(Width / 174);
    }
}