using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using PoolScoreBuddy.Resources;
using System.Collections.ObjectModel;
using PoolScoreBuddy.Domain.Services;
using PoolScoreBuddy.Domain.Models.API;
using PoolScoreBuddy.Domain.Models;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using PoolScoreBuddy.Domain;


namespace PoolScoreBuddy.ViewModels;
public partial class PlayerViewModel(ITournamentService tournamentService,
    IMessenger messenger,
    IScoreAPIClient cueScoreService,
    IEnsureConnectivity ensureConnectivity,
    ISettingsResolver settingsResolver,
    INotificationsChallenger notificationsChallenger,
    IAlert alert,
    ILogger<PlayerViewModel> logger) : BaseViewModel, IQueryAttributable
{

    ITournamentDecorator _tournament = null!;

    [ObservableProperty]
    private ObservableCollection<Player> players = [];

    [ObservableProperty]
    private bool isRefreshing;

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        _tournament = tournamentService.GetTournamentById(Convert.ToInt32(query["TournamentId"]));
    }

    [RelayCommand]
    internal async Task Refresh()
    {
        if (!await ensureConnectivity.IsConnectedWithAlert() || IsBusy) return;

        IsBusy = true;

        IEnumerable<Player> refreshedPlayers = [];

        try
        {
            var settings = settingsResolver.GetSettings();
            PlayersDto dto = new()
            {
                FallbackAddress = settings.CueScoreBaseUrl,
                BaseAddresses = settings.APIWebAppProxies,
                TournamentId = _tournament.Tournament.TournamentId,
            };

            var players = await cueScoreService.GetPlayers(dto);

            if (players.Error != null)
            {
                logger.LogError(players.Error);

                await alert.Show(string.Format(AppResources.PlayersAPIServerExceptionTitle, _tournament!.Tournament.TournamentId),
                    string.Format(AppResources.PlayersAPIServerExceptionMessage, players.Error),
                    AppResources.PlayersAPIServerExceptionButton);

                IsBusy = false;
            }
            else
            {
                _tournament.Players = await cueScoreService.GetPlayers(dto);
                refreshedPlayers = _tournament.GetPlayersWithMonitoring();

                Players.Clear();

                foreach (var player in refreshedPlayers)
                {
                    Players.Add(player);
                }
            }
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Error fetching players");

            await alert.Show(string.Format(AppResources.PlayersHttpExceptionTitle, _tournament!.Tournament.TournamentId),
                AppResources.PlayersHttpExceptionMessage,
                AppResources.PlayersHttpExceptionButton);
        }
        catch (JsonException ex)
        {
            logger.LogError(ex, "Error fetching players");

            await alert.Show(string.Format(AppResources.PlayersJsonExceptionTitle, _tournament!.Tournament.TournamentId),
                string.Format(AppResources.PlayersJsonExceptionMessage, _tournament!.Tournament.TournamentId, ex.Message),
                AppResources.PlayersJsonExceptionButton);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching players");

            await alert.Show(string.Format(AppResources.PlayersGeneralExceptionTitle, _tournament!.Tournament.TournamentId),
                string.Format(AppResources.PlayersGeneralExceptionMessage, _tournament!.Tournament.TournamentId, ex.Message),
                AppResources.PlayersGeneralExceptionButton);
        }

        IsBusy = false;
    }

    [RelayCommand]
    internal async Task Appearing()
    {
        await Refresh();
    }

    public Command<Player> ToggleStartMonitor => new(async (player) =>
    {
        if (!player.IsMonitored && MaximumMonitorCountReached())
        {
            await alert.Show(AppResources.Alert,
                AppResources.PlayersMaximumReached,
                AppResources.OK);
            return;
        }

        var notificationsAllowed = await notificationsChallenger.AllowNotificationsAsync();

        if (player == null || !notificationsAllowed)
            return;

        player.MonitoredPlayer = _tournament.TogglePlayerEnabled(player.PlayerId);

        int playerIndex = Players.IndexOf(Players.First(p => p.PlayerId == player.PlayerId));
        Players[playerIndex] = player;

        tournamentService.AddIfMissing(_tournament);

        messenger.Send(new CuescoreBackgroundChecker(ServiceMessageType.Default));
    });

    private bool MaximumMonitorCountReached()
    {
        return _tournament.MonitoredPlayers.Count >= Constants.MaximumMonitoredPlayersPerTournament;
    }
}
