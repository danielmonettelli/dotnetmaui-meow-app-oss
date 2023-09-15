namespace Meow.Views;

public partial class VotePage : ContentPage
{
    private readonly VoteViewModel vm = new(new CatService());

    public VotePage()
    {
        InitializeComponent();

        BindingContext = vm;
    }

    //protected async override void OnAppearing()
    //{
    //    base.OnAppearing();

    //    await vm.InitializeDataAsync();
    //}
}