using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PoolScoreBuddy.Models.API;
using PoolScoreBuddy.Resources;
using System.Text.Json;

namespace PoolScoreBuddy.ViewModels;
public partial class TournamentViewModel(IScoreAPIClient scoreAPIClient) : BaseViewModel
{
    readonly IScoreAPIClient _scoreAPIClient = scoreAPIClient;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(TournamentLoadCommand))]
    string? tournamentId = "";

    [ObservableProperty]
    string? errorMessage;

    public event EventHandler? FocusView;

    private bool CanExecuteTournamentLoad() => !string.IsNullOrEmpty(TournamentId);

    [RelayCommand(CanExecute = nameof(CanExecuteTournamentLoad))]
    private async Task TournamentLoad()
    {
        try
        {
            IsBusy = true;

            Tournament? tournament = await _scoreAPIClient.GetTournament(Convert.ToInt32(TournamentId));

            if (tournament != null)
            {
                TournamentDecorator tournamentDecorator = new(tournament);

                IsBusy = false;
                await GoToTournamentSelectedPage(tournamentDecorator);
            }
        }
        catch (HttpRequestException)
        {
            await Application.Current!.MainPage!.DisplayAlert(string.Format(AppResources.TournamentHttpExceptionTitle, TournamentId),
                AppResources.TournamentHttpExceptionMessage,
                AppResources.TournamentHttpExceptionButton);

            FocusView?.Invoke(this, EventArgs.Empty);
        }
        catch (APIServerException ex)
        {
            await Application.Current!.MainPage!.DisplayAlert(string.Format(AppResources.TournamentAPIServerExceptionTitle, TournamentId), 
                string.Format(AppResources.TournamentAPIServerExceptionMessage, ex!.Message),
                AppResources.TournamentAPIServerExceptionButton);

            FocusView?.Invoke(this, EventArgs.Empty);
        }
        catch (JsonException ex)
        {
            await Application.Current!.MainPage!.DisplayAlert(string.Format(AppResources.TournamentJsonExceptionTitle, TournamentId),
                string.Format(AppResources.TournamentJsonExceptionMessage, TournamentId, ex.Message),
                AppResources.TournamentJsonExceptionButton);
            
            FocusView?.Invoke(this, EventArgs.Empty);
        }

        IsBusy = false;
    }

    private async Task GoToTournamentSelectedPage(TournamentDecorator tournament)
    {
        var navigationParameters = new Dictionary<string, object>
        {
            { "Tournament", tournament },
        };

        await Shell.Current.GoToAsync(nameof(TournamentSelectedPage), false, navigationParameters);
    }
}
