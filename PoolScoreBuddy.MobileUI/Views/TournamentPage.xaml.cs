using PoolScoreBuddy.Domain.Services;

namespace PoolScoreBuddy.Views;

public partial class TournamentPage : ContentPage
{
    private readonly TournamentViewModel _viewModel;

    public TournamentPage()
    {
        var scoreAPIClient = ServiceResolver.GetService<IScoreAPIClient>();
        var ensureConnectivity = ServiceResolver.GetService<IEnsureConnectivity>();
        var alert = ServiceResolver.GetService<IAlert>();
        var appShell = ServiceResolver.GetService<IPoolAppShell>();


        InitializeComponent();
        _viewModel = new TournamentViewModel(scoreAPIClient, ensureConnectivity, alert, appShell);
        BindingContext = _viewModel;
        _viewModel.FocusView += TournamentEntrySetFocus;
    }

    private void TournamentEntrySetFocus(object? sender, EventArgs e)
    {
        TournamentEntry.Focus();
    }
}