//using AsyncAwaitBestPractices;
using CommunityToolkit.Mvvm;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using MauiApp3.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace CuescoreBuddy.ViewModels;
public partial class ParticipantViewModel : ObservableObject
{
    public ObservableCollection<Player> Participants { get; private set; } = [];

    readonly DataStore? DataStore;
    readonly IMessenger _messenger;
    TournamentFacade _tournament;

    bool isRefreshing;
    //public ICommand RefreshCommand => new Command(async () => await RefreshItemsAsync());

    //public ICommand ToggledCommand => new Command(async () => await RefreshItemsAsync());


    public ParticipantViewModel(DataStore dataStore, IMessenger messenger)
    {
        DataStore = dataStore;
        _messenger = messenger;

        _tournament = DataStore.Tournaments.GetTournamentById(DataStore.CurrentTournamentId);

        //RefreshItemsAsync().SafeFireAndForget();
    }


    public bool IsRefreshing
    {
        get { return isRefreshing; }
        set { SetProperty(ref isRefreshing, value); }
    }

    async Task RefreshItemsAsync()
    {
        IsRefreshing = true;
        Participants.Clear();

        var cueScoreService = ServiceResolver.GetService<ICueScoreService>();

        try
        {
            await _tournament.Fetch(cueScoreService, DataStore.CurrentTournamentId);
            DataStore.Tournaments.AddIfMissing(_tournament);

            await _tournament.LoadPlayers(cueScoreService);

        }
        catch (Exception ex)
        {
            throw;
        }

        Participants.Clear();

        var players = _tournament.GetLoadedPlayers().ToList();

        foreach (var player in players)
        {
            Participants.Add(player);
        }

        IsRefreshing = false;
    }

    #region Commands
    public Command<Player> ParticipantTapped => new(async (participant) =>
    {
        if (participant == null)
            return;

        participant.MonitoredPlayer = _tournament.TogglePlayerEnabled(participant.playerId);

        // Remove and add to force UI update
        int playerIndex = Participants.IndexOf(Participants.First(p => p.playerId == participant.playerId));
        Participants[playerIndex] = participant;

        _messenger.Send(new CuescoreBackgroundChecker()); //TODO create tournament / player state object

        string addingRemoving = participant.IsMonitored ? "Adding" : "Removing";
        await Shell.Current.DisplayAlert($"Monitoring change", $"{addingRemoving} {participant.name} from tournament {_tournament.Tournament.name}.", "OK");

        //RefreshSelectedParticipants();
        //await Shell.Current.GoToAsync($"{nameof(ConfirmationPage)}?id={participant.playerId}");
    });

    public Command ParticipantTapped2 => new(async () =>
    {
        Console.WriteLine("123");
        //if (participant == null)
        //    return;

        //await Shell.Current.GoToAsync($"{nameof(ConfirmationPage)}?id={participant.playerId}");
    });
    #endregion
}
