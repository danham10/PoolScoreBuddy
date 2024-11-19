using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using CuescoreBuddy.Models;
using CuescoreBuddy.Services;
using CuescoreBuddy.Views;
using System.Runtime.CompilerServices;

namespace CuescoreBuddy.ViewModels;
public partial class TournamentSelectedViewModel : BaseViewModel, IQueryAttributable
{
    readonly DataStore _dataStore;
    readonly IMessenger _messenger;

    int _tournamentId;
    TournamentFacade? _tournament;

    [ObservableProperty]
    public string tournamentName;

    public Command SaveCommand { get; private set; }

    public TournamentSelectedViewModel()
    {
        Title = "About";
        Description = "51123145";  //50522059

        //TODO On activating app paste in clipboard (if tournament) to textbox

        SaveCommand = new Command(async () => await ExecuteSave(), CanExecuteSave);
        PropertyChanged += (_, __) => SaveCommand.ChangeCanExecute();
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        _tournament = (query["Tournament"] as TournamentFacade);
        TournamentName = _tournament.Tournament.name;

        query.Clear();
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
        return !_tournament.IsFinished();
    }
    private async Task ExecuteSave()
    {
        await GoToPlayerPage(_tournament);
    }

    private async Task GoToPlayerPage(TournamentFacade tournament)
    {
        var navigationParameters = new Dictionary<string, object>
        {
            { "Tournament", tournament },
        };

        await Shell.Current.GoToAsync(nameof(PlayerPage), false, navigationParameters);
    }
    #endregion
}
