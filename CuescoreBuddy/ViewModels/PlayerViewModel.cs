//using AsyncAwaitBestPractices;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Plugin.LocalNotification;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CuescoreBuddy.ViewModels;
public partial class PlayerViewModel : BaseViewModel, IQueryAttributable
{
    readonly DataStore _dataStore;
    readonly IMessenger _messenger;
    readonly ICueScoreService _cueScoreService;

    TournamentFacade? _tournament;
    bool _isRefreshing;

    public ObservableCollection<Player> Players { get; private set; } = [];

    [ObservableProperty]
    private bool isRefreshing;

    public ICommand RefreshCommand => new AsyncRelayCommand(RefreshPlayersAsync);

    public PlayerViewModel()
    {
        _dataStore = ServiceResolver.GetService<DataStore>();
        _messenger = ServiceResolver.GetService<IMessenger>();
        _cueScoreService = ServiceResolver.GetService<ICueScoreService>();

    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        _tournament = (query["Tournament"] as TournamentFacade);
        //query.Clear();
    }

    async static Task<bool> AllowNotificationsAsync()
    {
        var allowed = await LocalNotificationCenter.Current.AreNotificationsEnabled();

        if (!allowed)
            allowed = await LocalNotificationCenter.Current.RequestNotificationPermission();

        if (!allowed)
        {
            await Application.Current.MainPage.DisplayAlert("Alert", "You must manually allow notifications for this app to work properly. Go to app settings, then permissions and under the 'not allowed' list, modify the 'Notifications' entry to become allowed.", "OK");
        }
            
        return allowed;
    }

    async Task RefreshPlayersAsync()
    {
        IsBusy = true;

        //await _tournament.LoadPlayers(_cueScoreService);

        Players.Clear();

        var players = await _tournament.GetLoadedPlayers(_cueScoreService);

        foreach (var player in players)
        {
            Players.Add(player);
        }

        IsBusy = false;
    }

    [RelayCommand]
    async Task Appearing()
    {
        if (!Players.Any())
            await RefreshPlayersAsync();
    }

    public Command<Player> ToggleStartMonitor => new(async (player) =>
    {
        var notificationsAllowed = await AllowNotificationsAsync();

        if (player == null || !notificationsAllowed)
            return;

        player.MonitoredPlayer = _tournament!.TogglePlayerEnabled(player.playerId);

        int playerIndex = Players.IndexOf(Players.First(p => p.playerId == player.playerId));
        Players[playerIndex] = player;

        _dataStore.Tournaments.AddIfMissing(_tournament);

        _messenger.Send(new CuescoreBackgroundChecker(ServiceMessageType.Default));
    });
}
