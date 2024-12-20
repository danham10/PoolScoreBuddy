using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using PoolScoreBuddy.Domain.Models;
using PoolScoreBuddy.Domain.Models.API;
using PoolScoreBuddy.Domain.Services;
using PoolScoreBuddy.Resources;
using System.Diagnostics;
using System.Text.Json;

namespace PoolScoreBuddy.ViewModels;
public partial class TournamentViewModel(IScoreAPIClient scoreAPIClient,
    IDataStore dataStore,
    IEnsureConnectivity ensureConnectivity, 
    IAlert alert,
    IPoolAppShell appShell,
    ISettingsResolver settingsResolver,
    ILogger<TournamentViewModel> logger) : BaseViewModel
{
    readonly IScoreAPIClient _scoreAPIClient = scoreAPIClient;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(TournamentLoadCommand))]
    string? tournamentId = "";

    [ObservableProperty]
    string? errorMessage;

    public event EventHandler? FocusView;

    private bool CanExecuteTournamentLoad() 
    {


        return !string.IsNullOrEmpty(TournamentId); 
    }

    [RelayCommand(CanExecute = nameof(CanExecuteTournamentLoad))]
    private async Task TournamentLoad()
    {
        try
        {
            if (!await ensureConnectivity.IsConnectedWithAlert()) return;

            var settings = settingsResolver.GetSettings();
            IsBusy = true;

            Tournament tournament = await GetTournament();

            if (tournament != null)
            {
                TournamentDecorator tournamentDecorator = new(tournament);
                dataStore.Tournaments.AddIfMissing(tournamentDecorator);

                await GoToTournamentSelectedPage(tournamentDecorator);
            }
        }
        catch (HttpRequestException ex)
        {
            switch (ex.StatusCode)
            {
                case System.Net.HttpStatusCode.Unauthorized:
                    await alert.Show(string.Format(AppResources.TournamentHttpExceptionTitle, TournamentId),
                       AppResources.TournamentHttpExceptionUnauthorisedMessage,
                       AppResources.TournamentHttpExceptionButton);
                    break;

                default:
                    await alert.Show(string.Format(AppResources.TournamentHttpExceptionTitle, TournamentId),
                       AppResources.TournamentHttpExceptionMessage,
                       AppResources.TournamentHttpExceptionButton);
                    break;
            }
        }
        catch (APIServerException ex)
        {
            logger.LogError(ex, "APIServerException");

            await alert.Show(string.Format(AppResources.TournamentAPIServerExceptionTitle, TournamentId), 
                string.Format(AppResources.TournamentAPIServerExceptionMessage, ex!.Message),
                AppResources.TournamentAPIServerExceptionButton);
        }
        catch (JsonException ex)
        {
            logger.LogError(ex, "JsonException");

            await alert.Show(string.Format(AppResources.TournamentJsonExceptionTitle, TournamentId),
                string.Format(AppResources.TournamentJsonExceptionMessage, TournamentId, ex.Message),
                AppResources.TournamentJsonExceptionButton);
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "Exception");

            await alert.Show(string.Format(AppResources.TournamentGeneralExceptionTitle, TournamentId),
                string.Format(AppResources.TournamentGeneralExceptionMessage, TournamentId, ex.Message),
                AppResources.TournamentGeneralExceptionButton);
        }
        finally
        {
            IsBusy = false;
            FocusView?.Invoke(this, EventArgs.Empty);
        }
    }

    private async Task<Tournament> GetTournament()
    {
        var settings = settingsResolver.GetSettings();

        TournamentDto dto = new()
        {
            BaseAddresses = settings.APIWebAppProxies,
            FallbackAddress = settings.CueScoreBaseUrl,
            TournamentId = Convert.ToInt32(TournamentId),
            PlayerIds = [],
        };

        Tournament? tournament = await _scoreAPIClient.GetTournament(dto);
        return tournament;
    }

    private async Task GoToTournamentSelectedPage(TournamentDecorator tournament)
    {
        var navigationParameters = new Dictionary<string, object>
        {
            { "TournamentId", tournament.Tournament.TournamentId! },
        };
        await appShell.GoToAsync(nameof(TournamentSelectedPage), navigationParameters);
    }
}

