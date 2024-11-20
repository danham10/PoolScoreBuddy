namespace CuescoreBuddy.Views;

public partial class PlayerPage : ContentPage
{
	public PlayerPage()
	{
		InitializeComponent();
	}

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (!Navigation.NavigationStack.Contains(this))
        {
            Console.WriteLine("");
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        if (!Navigation.NavigationStack.Contains(this))
        {
            Console.WriteLine("");
        }
    }
}