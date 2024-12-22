using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using PoolScoreBuddy.Domain.Services;

namespace PoolScoreBuddy.Views;

public partial class PlayerPage : ContentPage
{
    private readonly PlayerViewModel _viewModel;

    public PlayerPage(ITournamentService tournamentService, 
        IMessenger messenger, 
        IScoreAPIClient cueScoreService, 
        IEnsureConnectivity ensureConnectivity,
        ISettingsResolver settingsResolver,
        INotificationsChallenger notificationsChallenger,
        IAlert alert,
        ILogger<PlayerViewModel> logger)
	{
		InitializeComponent();

        _viewModel = new PlayerViewModel(tournamentService, 
            messenger, 
            cueScoreService, 
            ensureConnectivity, 
            settingsResolver, 
            notificationsChallenger,
            alert,
            logger
            );  

        BindingContext = _viewModel;
    }
}