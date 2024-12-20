using PoolScoreBuddy.Domain.Services;

namespace PoolScoreBuddy.Views;

public partial class TournamentPage : ContentPage
{
    private readonly TournamentViewModel _viewModel;

    public TournamentPage(IScoreAPIClient scoreAPIClient, IEnsureConnectivity ensureConnectivity, IAlert alert, IPoolAppShell appShell, ISettingsResolver settingsResolver)
    {
        InitializeComponent();
        _viewModel = new TournamentViewModel(scoreAPIClient, ensureConnectivity, alert, appShell, settingsResolver);
        BindingContext = _viewModel;
        _viewModel.FocusView += TournamentEntrySetFocus;
    }

    private void TournamentEntrySetFocus(object? sender, EventArgs e)
    {
        TournamentEntry.Focus();
    }
}