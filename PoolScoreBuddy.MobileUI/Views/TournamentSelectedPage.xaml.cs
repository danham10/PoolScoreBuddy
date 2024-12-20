namespace PoolScoreBuddy.Views;

public partial class TournamentSelectedPage : ContentPage
{
    private readonly TournamentSelectedViewModel _viewModel;

    public TournamentSelectedPage(IDataStore dataStore, IPoolAppShell appShell)
    {
        InitializeComponent();

        _viewModel = new TournamentSelectedViewModel(dataStore, appShell);

        BindingContext = _viewModel;
    }
}