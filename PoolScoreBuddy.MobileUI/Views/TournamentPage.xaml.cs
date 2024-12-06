using Microsoft.Extensions.Configuration;
using PoolScoreBuddy.Domain.Services;

namespace PoolScoreBuddy.Views;

public partial class TournamentPage : ContentPage
{
    private readonly TournamentViewModel _viewModel;

    public TournamentPage()
    {
        var scoreAPIClient = ServiceResolver.GetService<IScoreAPIClient>();
        var configuration = ServiceResolver.GetService<IConfiguration>();

        InitializeComponent();
        _viewModel = new TournamentViewModel(scoreAPIClient, configuration);
        BindingContext = _viewModel;
        _viewModel.FocusView += TournamentEntrySetFocus;
    }

    private void TournamentEntrySetFocus(object? sender, EventArgs e)
    {
        TournamentEntry.Focus();
    }
}