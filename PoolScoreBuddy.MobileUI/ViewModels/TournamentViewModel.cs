using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using PoolScoreBuddy.Domain.Models;
using PoolScoreBuddy.Domain.Models.API;
using PoolScoreBuddy.Domain.Services;
using PoolScoreBuddy.Resources;
using System.Text.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PoolScoreBuddy.ViewModels;
public partial class TournamentViewModel(IScoreAPIClient scoreAPIClient,
    ITournamentService tournamentService,
    IEnsureConnectivity ensureConnectivity, 
    IAlert alert,
    IPoolAppShell appShell,
    ISettingsResolver settingsResolver,
    ILogger<TournamentViewModel> logger) : BaseViewModel
{

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(TournamentLoadCommand))]
    string? tournamentId = "";

    [ObservableProperty]
    string? errorMessage;

    public event EventHandler? FocusView;

    public bool CanExecuteTournamentLoad() 
    {


        return !string.IsNullOrEmpty(TournamentId); 
    }

    [RelayCommand(CanExecute = nameof(CanExecuteTournamentLoad))]
    public async Task TournamentLoad()
    {
        try
        {
            if (!await ensureConnectivity.IsConnectedWithAlert()) return;

            var settings = settingsResolver.GetSettings();
            IsBusy = true;

            Tournament tournament = await GetTournament();

            if (tournament.Error != null)
            {
                logger.LogError(tournament.Error);

                await alert.Show(string.Format(AppResources.TournamentAPIServerExceptionTitle, TournamentId),
                    string.Format(AppResources.TournamentAPIServerExceptionMessage, tournament.Error),
                    AppResources.TournamentAPIServerExceptionButton);

                    IsBusy = false;
                    FocusView?.Invoke(this, EventArgs.Empty);
            } 
            else
            {
                TournamentDecorator tournamentDecorator = new(tournament);
                tournamentService.AddIfMissing(tournamentDecorator);

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

    internal async Task<Tournament> GetTournament()
    {
        var settings = settingsResolver.GetSettings();

        TournamentDto dto = new()
        {
            BaseAddresses = settings.APIWebAppProxies,
            FallbackAddress = settings.CueScoreBaseUrl,
            TournamentId = Convert.ToInt32(TournamentId),
            PlayerIds = [],
        };

        Tournament? tournament = await scoreAPIClient.GetTournament(dto);
        return tournament;
    }

    internal async Task GoToTournamentSelectedPage(TournamentDecorator tournament)
    {
        var navigationParameters = new Dictionary<string, object>
        {
            { "TournamentId", tournament.Tournament.TournamentId },
        };

        await appShell.GoToAsync(nameof(TournamentSelectedPage), navigationParameters);
    }
}

