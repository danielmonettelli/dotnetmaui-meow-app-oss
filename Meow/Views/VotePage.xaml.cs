namespace Meow.Views;

public partial class VotePage : ContentPage
{
    private readonly VoteViewModel vm;

    public VotePage(VoteViewModel voteViewModel)
    {
        InitializeComponent();
        vm = voteViewModel;

        BindingContext = vm;
    }

    //protected async override void OnAppearing()
    //{
    //    base.OnAppearing();

    //    await vm.InitializeDataAsync();
    //}
}