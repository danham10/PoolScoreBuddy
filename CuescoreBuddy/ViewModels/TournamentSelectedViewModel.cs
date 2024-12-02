using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CuescoreBuddy.Models.API;
using CuescoreBuddy.Resources;

namespace CuescoreBuddy.ViewModels;
public partial class TournamentSelectedViewModel : BaseViewModel, IQueryAttributable
{
    TournamentDecorator? _tournament;

    [ObservableProperty]
    public required string tournamentName;

    [ObservableProperty]
    public string message = "";

    public TournamentSelectedViewModel()
    {
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        _tournament = query["Tournament"] as TournamentDecorator;

        TournamentName = _tournament!.Tournament!.Name!;
        Message = _tournament!.IsFinished() ?
            string.Format(AppResources.TournamentFinishedLabel, TournamentName) :
            string.Format(AppResources.TournamentSelectedLabel, TournamentName);
    }

    #region Commands

    private bool CanExecuteMonitor()
    {
        return _tournament!.IsFinished() == false;
    }

    [RelayCommand(CanExecute = nameof(CanExecuteMonitor))]
    private async Task Monitor()
    {
        var navigationParameters = new Dictionary<string, object>
        {
            { "Tournament", _tournament! },
        };

        await Shell.Current.GoToAsync(nameof(PlayerPage), false, navigationParameters);
    }

    #endregion
}
