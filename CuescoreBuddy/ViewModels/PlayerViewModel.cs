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

    public ICommand RefreshCommand => new Command(async () => await RefreshPlayersAsync());
    

    public PlayerViewModel()
    {
        _dataStore = ServiceResolver.GetService<DataStore>();
        _messenger = ServiceResolver.GetService<IMessenger>();
        _cueScoreService = ServiceResolver.GetService<ICueScoreService>();
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        _tournament = (query["Tournament"] as TournamentFacade);
        query.Clear();
    }

    async static Task<bool> AllowNotificationsAsync()
    {
        var allowed = await LocalNotificationCenter.Current.AreNotificationsEnabled();

        if (!allowed)
            allowed = await LocalNotificationCenter.Current.RequestNotificationPermission();

        if (!allowed)
        {
            //TODO https://stackoverflow.com/questions/72429055/how-to-displayalert-in-a-net-maui-viewmodel
            await Application.Current.MainPage.DisplayAlert("Alert", "You must manually allow notifications for this app to work properly. Go to app settings, then permissions and under the 'not allowed' list, modify the 'Notifications' entry to become allowed.", "OK");

            //var request = new NotificationRequest()
            //{
            //    NotificationId = 100,
            //    Title = "Notifications permission declined",
            //    Description = "You must manually allow notifications for this app to work properly. Go to app settings, then permissions and under the 'not allowed' list, modify the 'Notifications' entry to become allowed."
            //};
            //await LocalNotificationCenter.Current.Show(request);
        }
            
        return allowed;
    }

    async Task RefreshPlayersAsync()
    {
        IsBusy = true;

        await _tournament.LoadPlayers(_cueScoreService);

        Players.Clear();

        var players = _tournament.GetLoadedPlayers().ToList();

        foreach (var player in players)
        {
            Players.Add(player);
        }

        IsBusy = false;
    }

    #region Commands

    [RelayCommand]
    async Task Appearing()
    {
        try
        {
            if (!Players.Any())
                await RefreshPlayersAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex.ToString());
        }
    }

    //[RelayCommand]
    //void Disappearing()
    //{
    //    try
    //    {
    //        // DoSomething

    //    }
    //    catch (Exception ex)
    //    {
    //        System.Diagnostics.Debug.WriteLine(ex.ToString());
    //    }
    //}

    public Command<Player> ToggleScoreMonitor => new(async (player) =>
    {
    });

    public Command<Player> ToggleStartMonitor => new(async (player) =>
    {
        var notificationsAllowed = await AllowNotificationsAsync();

        if (player == null || !notificationsAllowed)
            return;

        player.MonitoredPlayer = _tournament!.TogglePlayerEnabled(player.playerId);

        // Remove and add to force UI update
        int playerIndex = Players.IndexOf(Players.First(p => p.playerId == player.playerId));
        Players[playerIndex] = player;

        _dataStore.Tournaments.AddIfMissing(_tournament);

        _messenger.Send(new CuescoreBackgroundChecker(ServiceMessageType.Default));
    });
    #endregion
}
