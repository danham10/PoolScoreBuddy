using CommunityToolkit.Maui.Core.Platform;

namespace PoolScoreBuddy.Views;

public partial class TournamentPage : ContentPage
{
    private readonly TournamentViewModel _viewModel;

    public TournamentPage()
    {
        var scoreAPIClient = ServiceResolver.GetService<IScoreAPIClient>();

        InitializeComponent();
        _viewModel = new TournamentViewModel(scoreAPIClient);
        BindingContext = _viewModel;
        _viewModel.FocusView += TournamentEntrySetFocus;
    }

    private void TournamentEntrySetFocus(object? sender, EventArgs e)
    {
        TournamentEntry.Focus();
    }
}