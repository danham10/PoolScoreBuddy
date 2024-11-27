using CommunityToolkit.Maui.Core.Platform;

namespace CuescoreBuddy.Views;

public partial class TournamentPage : ContentPage
{
    private TournamentViewModel _viewModel;

    public TournamentPage()
	{
        InitializeComponent();
        _viewModel = new TournamentViewModel();
        BindingContext = _viewModel;
        _viewModel.FocusView += TournamentEntrySetFocus;
    }

    private void TournamentEntrySetFocus(object? sender, EventArgs e)
    {
        TournamentEntry.Focus();
    }
}