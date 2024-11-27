using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CuescoreBuddy.Models.API;

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
    private async static Task TapCommand(string url)
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
        catch (HttpRequestException)
        {
            await Application.Current!.MainPage!.DisplayAlert($"Cannot fetch tournament {TournamentId}", "Check you have a data connection", "OK");
            FocusView?.Invoke(this, EventArgs.Empty);
        }
        catch (APIException ex)
        {
            await Application.Current!.MainPage!.DisplayAlert($"Cannot fetch tournament {TournamentId}.", ex.Message, "OK");
            FocusView?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception)
        {
            await Application.Current!.MainPage!.DisplayAlert($"Cannot fetch tournament {TournamentId}", $"If the number is correct, please consider emailing me your tournament number {TournamentId} at poolscorebuddy@outlook.com so I can look into fixing that. Thankyou.", "OK");
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
