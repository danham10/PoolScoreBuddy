using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;

namespace CuescoreBuddy.ViewModels;
public partial class TournamentViewModel : BaseViewModel
{
    [ObservableProperty]
    private string? tournamentId;

    [ObservableProperty]
    private string? errorMessage;

    public ICommand SaveCommand { get; private set; }
    public ICommand PasteCommand { get; private set; }

    public TournamentViewModel()
    {
        Title = "About";
        TournamentId = "";  //50522059
        SaveCommand = new AsyncRelayCommand(ExecuteTournamentLoadAsync, CanExecuteTournamentLoad);
        PasteCommand = new AsyncRelayCommand(ExecutePasteAsync);
    }


    //[RelayCommand]
    //async Task Appearing()
    //{

    //    if (Clipboard.Default.HasText)
    //    {
    //        var clipboard = await Clipboard.Default.GetTextAsync();
    //        bool clipboardEndsInNumber = char.IsDigit(clipboard.Last());
    //        TournamentId = await Clipboard.Default.GetTextAsync();
    //    }
    //}

    //private bool CanExecutePaste()
    //{
    //    try
    //    {
    //        if (Clipboard.Default.HasText)
    //        {
    //            var clipboard = Clipboard.Default.GetTextAsync().Result;
    //            bool clipboardEndsInNumber = char.IsDigit(clipboard.Last());
    //            return clipboard.ToLower().Contains("cuescore") && clipboardEndsInNumber;
    //        }
    //    }
    //    catch (Exception)
    //    {
    //    }
    //    return false;
    //}

    private async Task ExecutePasteAsync()
    {
        if (Clipboard.Default.HasText)
        {
            var clipboard = Clipboard.Default.GetTextAsync().Result;
            bool clipboardEndsInNumber = char.IsDigit(clipboard.Last());
            if (clipboard.ToLower().Contains("cuescore") && clipboardEndsInNumber)
            {
                TournamentId = GetNumberAtEndOfString(clipboard);
                await ExecuteTournamentLoadAsync();
            }
            else
                await Application.Current.MainPage.DisplayAlert("Alert", "No CueScore tournament found in clipboard.", "OK");

        }
    }


    private bool CanExecuteTournamentLoad()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(TournamentId)) return false;

            TournamentId = GetNumberAtEndOfString(TournamentId);

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    private async Task ExecuteTournamentLoadAsync()
    {
        string error = "";

        IsBusy = true;

        int tournamentId = Convert.ToInt32(GetNumberAtEndOfString(TournamentId));

        var cueScoreService = ServiceResolver.GetService<ICueScoreService>();
        Tournament? tournament = await cueScoreService.GetTournament(tournamentId);

        if (tournament != null)
        {
            TournamentFacade tournamentFacade = new(tournament);

            IsBusy = false;

            await GoToTournamentSelectedPage(tournamentFacade);
        } else
        {
            ErrorMessage = "Cannot load tournament";
        }
    }

    private async Task GoToTournamentSelectedPage(TournamentFacade tournament)
    {
        var navigationParameters = new Dictionary<string, object>
        {
            { "Tournament", tournament },
        };

        await Shell.Current.GoToAsync(nameof(TournamentSelectedPage), false, navigationParameters);
    }

    private string GetNumberAtEndOfString(string value) => string.Concat(value.ToArray().Reverse().TakeWhile(char.IsNumber).Reverse());
}
