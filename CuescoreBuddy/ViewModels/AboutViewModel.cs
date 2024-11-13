using CuescoreBuddy.Models;
using CuescoreBuddy.Services;
using CuescoreBuddy.Views;
using System.Runtime.CompilerServices;

namespace CuescoreBuddy.ViewModels;
public class AboutViewModel : BaseViewModel
{
    public string Platform { get; } = DeviceInfo.Platform.ToString();
    public string Version { get; } = DeviceInfo.Version.ToString();
    public string DeviceName { get; } = DeviceInfo.Name;
    public string DeviceType { get; } = DeviceInfo.DeviceType.ToString();

    public Command SaveCommand { get; private set; }

    public AboutViewModel()
    {
        Title = "About";
        Description = "51818677";  //50522059

        //TODO On activating app paste in clipboard (if tournament) to textbox

        SaveCommand = new Command(async () => await ExecuteSave(), CanExecuteSave);
        PropertyChanged += (_, __) => SaveCommand.ChangeCanExecute();
    }

    #region Fields
    private string? description;
    public string? Description
    {
        get => description;
        set => SetProperty(ref description, value);
    }
    #endregion

    #region Commands
    private bool CanExecuteSave()
    {
        try
        {
            Convert.ToInt32(Description);
        }
        catch (Exception)
        {
            return false;
        }

        return Description?.Length > 0;
    }

    private async Task ExecuteSave()
    {
        string error = "";

        IsBusy = true;

        var cueScoreService = ServiceResolver.GetService<ICueScoreService>();
        Tournament? tournament = await cueScoreService.GetTournament(Convert.ToInt32(Description));

        if (tournament != null)
        {

            //List<Participant>? participants = await cueScoreService.GetParticipants(Convert.ToInt32(Description));

            var dataStore = ServiceResolver.GetService<DataStore>();
            //dataStore.Participants = participants;
            dataStore.CurrentTournamentId = tournament.tournamentId;

            IsBusy = false;



            //await Shell.Current.GoToAsync(nameof(ParticipantPage));
            await GoToParticipantPage(tournament.tournamentId);
        }

        IsBusy = false;
        // TODO display error message



    }

    private async Task GoToParticipantPage(int tournamentId)
    {
        var navigationParameters = new Dictionary<string, object>
        {
            { "TournamentId", tournamentId },
        };

        await Shell.Current.GoToAsync(nameof(ParticipantPage), false, navigationParameters);
    }
    #endregion
}
