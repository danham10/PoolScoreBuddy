//using AsyncAwaitBestPractices;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CuescoreBuddy.Models.API;
using Plugin.LocalNotification;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CuescoreBuddy.ViewModels;
public partial class PlayerViewModel : BaseViewModel, IQueryAttributable
{
    readonly IDataStore _dataStore;
    readonly IMessenger _messenger;
    readonly IScoreAPIClient _cueScoreService;

    TournamentDecorator? _tournament;

    [ObservableProperty]
    private ObservableCollection<Player> players = [];

    [ObservableProperty]
    private bool isRefreshing;

    public ICommand RefreshCommand => new AsyncRelayCommand(RefreshPlayersAsync);

    public PlayerViewModel()
    {
        _dataStore = ServiceResolver.GetService<IDataStore>();
        _messenger = ServiceResolver.GetService<IMessenger>();
        _cueScoreService = ServiceResolver.GetService<IScoreAPIClient>();
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        _tournament = (query["Tournament"] as TournamentDecorator);
    }

    async static Task<bool> AllowNotificationsAsync()
    {
        var allowed = await LocalNotificationCenter.Current.AreNotificationsEnabled();

        if (!allowed)
            allowed = await LocalNotificationCenter.Current.RequestNotificationPermission();

        if (!allowed)
        {
            await Application.Current!.MainPage!.DisplayAlert("Alert", "You must manually allow notifications for this app to work properly. Go to app settings, then permissions and under the 'not allowed' list, modify the 'Notifications' entry to become allowed.", "OK");
        }
            
        return allowed;
    }

    async Task RefreshPlayersAsync()
    {
        IsBusy = true;

        Players.Clear();

        var refreshedPlayers =  await _tournament!.GetLoadedPlayers(_cueScoreService);

        foreach (var player in refreshedPlayers)
        {
            Players.Add(player);
        }

        IsBusy = false;
    }

    [RelayCommand]
    async Task Appearing()
    {
            await RefreshPlayersAsync();
    }

    public Command<Player> ToggleStartMonitor => new(async (player) =>
    {
        var notificationsAllowed = await AllowNotificationsAsync();

        if (player == null || !notificationsAllowed)
            return;

        player.MonitoredPlayer = _tournament!.TogglePlayerEnabled(player.PlayerId);

        int playerIndex = Players.IndexOf(Players.First(p => p.PlayerId == player.PlayerId));
        Players[playerIndex] = player;

        _dataStore.Tournaments.AddIfMissing(_tournament);

        _messenger.Send(new CuescoreBackgroundChecker(ServiceMessageType.Default));
    });
}
