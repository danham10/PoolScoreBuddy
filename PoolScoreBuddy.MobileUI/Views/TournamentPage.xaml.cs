using Microsoft.Extensions.Logging;
using PoolScoreBuddy.Domain.Services;

namespace PoolScoreBuddy.Views;

public partial class TournamentPage : ContentPage
{
    private readonly TournamentViewModel _viewModel;

    public TournamentPage(IScoreAPIClient scoreAPIClient,
        ITournamentService tournamentService,
        IEnsureConnectivity ensureConnectivity, 
        IAlert alert, 
        IPoolAppShell appShell, 
        ISettingsResolver settingsResolver,
        ILogger<TournamentViewModel> logger)
    {
        InitializeComponent();

        _viewModel = new TournamentViewModel(scoreAPIClient,
            tournamentService,
            ensureConnectivity, 
            alert, 
            appShell, 
            settingsResolver, 
            logger);

        BindingContext = _viewModel;

        _viewModel.FocusView += TournamentEntrySetFocus;
    }

    private void TournamentEntrySetFocus(object? sender, EventArgs e) => TournamentEntry.Focus();
}