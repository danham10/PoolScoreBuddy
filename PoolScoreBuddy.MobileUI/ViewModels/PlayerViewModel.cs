using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using PoolScoreBuddy.Resources;
using Plugin.LocalNotification;
using System.Collections.ObjectModel;
using PoolScoreBuddy.Domain.Services;
using PoolScoreBuddy.Domain.Models.API;
using PoolScoreBuddy.Domain.Models;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using PoolScoreBuddy.Domain;

namespace PoolScoreBuddy.ViewModels;
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
            await Application.Current!.MainPage!.DisplayAlert(AppResources.Alert, AppResources.ManualNotificationsWarning, "OK");
        }
            
        return allowed;
    }

    [RelayCommand]
    async Task Refresh()
    {
        if (!await EnsureConnectivity.IsConnectedWithAlert()) return;

        IsBusy = true;

        Players.Clear();

        IEnumerable<Player> refreshedPlayers = [];

        try
        {
            var settings = SettingsResolver.GetSettings();
            refreshedPlayers = await _tournament!.GetPlayers(_cueScoreService, settings.API.BaseUrl);
        }
        catch (HttpRequestException)
        {
            await Application.Current!.MainPage!.DisplayAlert(string.Format(AppResources.PlayersHttpExceptionTitle, _tournament!.Tournament.TournamentId),
                AppResources.PlayersHttpExceptionMessage,
                AppResources.PlayersHttpExceptionButton);
        }
        catch (APIServerException ex)
        {
            await Application.Current!.MainPage!.DisplayAlert(string.Format(AppResources.PlayersAPIServerExceptionTitle, _tournament!.Tournament.TournamentId),
                string.Format(AppResources.PlayersAPIServerExceptionMessage, ex!.Message),
                AppResources.PlayersAPIServerExceptionButton);
        }
        catch (JsonException ex)
        {
            await Application.Current!.MainPage!.DisplayAlert(string.Format(AppResources.TournamentJsonExceptionTitle, _tournament!.Tournament.TournamentId),
                string.Format(AppResources.PlayersJsonExceptionMessage, _tournament!.Tournament.TournamentId, ex.Message),
                AppResources.PlayersJsonExceptionButton);
        }

        foreach (var player in refreshedPlayers)
        {
            Players.Add(player);
        }

        IsBusy = false;
    }

    [RelayCommand]
    async Task Appearing()
    {
            await Refresh();
    }

    public Command<Player> ToggleStartMonitor => new(async (player) =>
    {
        if (MaximumMonitorCountReached())
        {
            await Application.Current!.MainPage!.DisplayAlert(string.Format(AppResources.PlayersHttpExceptionTitle, _tournament!.Tournament.TournamentId),
                AppResources.PlayersHttpExceptionMessage,
                AppResources.PlayersHttpExceptionButton);
            return;
        }

        var notificationsAllowed = await AllowNotificationsAsync();

        if (player == null || !notificationsAllowed)
            return;

        player.MonitoredPlayer = _tournament!.TogglePlayerEnabled(player.PlayerId);

        int playerIndex = Players.IndexOf(Players.First(p => p.PlayerId == player.PlayerId));
        Players[playerIndex] = player;

        _dataStore.Tournaments.AddIfMissing(_tournament);

        _messenger.Send(new CuescoreBackgroundChecker(ServiceMessageType.Default));
    });

    private bool MaximumMonitorCountReached()
    {
        return _tournament!.MonitoredPlayers.Count >= 10;
    }
}
