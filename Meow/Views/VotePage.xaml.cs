namespace Meow.Views;

public partial class VotePage : ContentPage
{
    public VotePage(VoteViewModel vm)
    {
        InitializeComponent();

        BindingContext = vm;
    }
}