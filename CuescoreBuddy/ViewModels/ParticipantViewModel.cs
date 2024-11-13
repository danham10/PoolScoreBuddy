//using AsyncAwaitBestPractices;
using AsyncAwaitBestPractices;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using MauiApp3.Models;
using Plugin.LocalNotification;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CuescoreBuddy.ViewModels;
public partial class ParticipantViewModel : BaseViewModel, IQueryAttributable
{
    readonly DataStore _dataStore;
    readonly IMessenger _messenger;

    int _tournamentId;
    TournamentFacade? _tournament;
    bool _isRefreshing;

    public ObservableCollection<Player> Participants { get; private set; } = [];

    [ObservableProperty]
    private bool isRefreshing;

    public ICommand RefreshCommand => new Command(async () => await RefreshItemsAsync());

    public ParticipantViewModel()
    {
        _dataStore = ServiceResolver.GetService<DataStore>();
        _messenger = ServiceResolver.GetService<IMessenger>();
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        _tournamentId = Convert.ToInt32(query["TournamentId"]);
        _tournament = _dataStore.Tournaments.GetTournamentById(_tournamentId);

        Participants.Clear();
        query.Clear();

        RefreshItemsAsync().SafeFireAndForget();
    }

    async Task RefreshItemsAsync()
    {
        var isenabled = await LocalNotificationCenter.Current.AreNotificationsEnabled();
        if (!isenabled) await LocalNotificationCenter.Current.RequestNotificationPermission();

                IsRefreshing = true;
       
        var cueScoreService = ServiceResolver.GetService<ICueScoreService>();

        await _tournament!.Fetch(cueScoreService, _tournamentId);
        _dataStore.Tournaments.AddIfMissing(_tournament);

        await _tournament.LoadPlayers(cueScoreService);

        Participants.Clear();

        var players = _tournament.GetLoadedPlayers().ToList();

        foreach (var player in players)
        {
            Participants.Add(player);
        }

        IsRefreshing = false;
    }

    #region Commands
    public Command<Player> ToggleScoreMonitor => new((participant) =>
    {
        //if (participant == null)
        //    return;

        //participant.MonitoredPlayer = _tournament!.TogglePlayerEnabled(participant.playerId);

        //// Remove and add to force UI update
        //int playerIndex = Participants.IndexOf(Participants.First(p => p.playerId == participant.playerId));
        //Participants[playerIndex] = participant;

        //_messenger.Send(new CuescoreBackgroundChecker()); //TODO create tournament / player state object
    });

    public Command<Player> ToggleStartMonitor => new((participant) =>
    {
        if (participant == null)
            return;

        participant.MonitoredPlayer = _tournament!.TogglePlayerEnabled(participant.playerId);

        // Remove and add to force UI update
        int playerIndex = Participants.IndexOf(Participants.First(p => p.playerId == participant.playerId));
        Participants[playerIndex] = participant;

        _messenger.Send(new CuescoreBackgroundChecker()); //TODO create tournament / player state object
    });
    #endregion
}
