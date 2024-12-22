using PoolScoreBuddy.Domain.Services;

namespace PoolScoreBuddy.Views;

public partial class TournamentSelectedPage : ContentPage
{
    private readonly TournamentSelectedViewModel _viewModel;

    public TournamentSelectedPage(ITournamentService tournamentService, IPoolAppShell appShell)
    {
        InitializeComponent();

        _viewModel = new TournamentSelectedViewModel(tournamentService, appShell);

        BindingContext = _viewModel;
    }
}