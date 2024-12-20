using CommunityToolkit.Mvvm.Messaging;
using PoolScoreBuddy.Domain.Services;

namespace PoolScoreBuddy.Views;

public partial class PlayerPage : ContentPage
{
    private readonly PlayerViewModel _viewModel;

    public PlayerPage(IDataStore dataStore, 
        IMessenger messenger, 
        IScoreAPIClient cueScoreService, 
        IEnsureConnectivity ensureConnectivity,
        ISettingsResolver settingsResolver,
        INotificationsChallenger notificationsChallenger)
	{
		InitializeComponent();
        _viewModel = new PlayerViewModel(dataStore, messenger, cueScoreService, ensureConnectivity, settingsResolver, notificationsChallenger);  
        BindingContext = _viewModel;
    }
}