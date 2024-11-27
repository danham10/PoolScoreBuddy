using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CuescoreBuddy.Models.API;
using System.Text.Json;

namespace CuescoreBuddy.ViewModels;
public partial class TournamentViewModel : BaseViewModel
{
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(TournamentLoadCommand))]
    private string? tournamentId = "";

    [ObservableProperty]
    private string? errorMessage;

    public event EventHandler? FocusView;

    public TournamentViewModel()
    {
        Title = "About";
    }


    [RelayCommand]
    private async Task TapCommand(string url)
    {
        await Launcher.OpenAsync(url);
    }

    private bool CanExecuteTournamentLoad() => !string.IsNullOrEmpty(TournamentId);

    [RelayCommand(CanExecute = nameof(CanExecuteTournamentLoad))]
    private async Task TournamentLoad()
    {
        try
        {
            IsBusy = true;

            var cueScoreService = ServiceResolver.GetService<IScoreAPIClient>();
            Tournament? tournament = await cueScoreService.GetTournament(Convert.ToInt32(TournamentId));

            if (tournament != null)
            {
                TournamentDecorator tournamentFacade = new(tournament);

                IsBusy = false;
                await GoToTournamentSelectedPage(tournamentFacade);
            }
        }
        catch (JsonException)
        {
            await Application.Current!.MainPage!.DisplayAlert($"Data error for {TournamentId}. This tournament is not yet supported.", $"Please consider emailing me your tournament number {TournamentId} at poolscorebuddy@outlook.com so I can look into fixing that. Thankyou.", "OK");
            FocusView?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception)
        {
            await Application.Current!.MainPage!.DisplayAlert($"Cannot load tournament {TournamentId}", "Check number matches the link you have been sent, and ensure network connectivity", "OK");
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
