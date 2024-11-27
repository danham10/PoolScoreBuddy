using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CuescoreBuddy.Models.API;
using System.Windows.Input;

namespace CuescoreBuddy.ViewModels;
public partial class TournamentSelectedViewModel : BaseViewModel, IQueryAttributable
{
    TournamentDecorator? _tournament;

    [ObservableProperty]
    public required string tournamentName;

    [ObservableProperty]
    public string message = "";

    public ICommand MonitorCommand { get; private set; }

    public TournamentSelectedViewModel()
    {
        Title = "About";
        MonitorCommand = new AsyncRelayCommand(ExecuteMonitorAsync, CanExecuteMonitor);
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        _tournament = (query["Tournament"] as TournamentDecorator);

        TournamentName = _tournament!.Tournament!.Name!;
        Message = _tournament!.IsFinished() ?
            $"'{TournamentName}' has finished. Go back and choose another." :
            $"'{TournamentName}' was found";
    }

    #region Commands

    private bool CanExecuteMonitor()
    {
        return _tournament!.IsFinished() == false;
    }

    private async Task ExecuteMonitorAsync()
    {
        var navigationParameters = new Dictionary<string, object>
        {
            { "Tournament", _tournament! },
        };

        await Shell.Current.GoToAsync(nameof(PlayerPage), false, navigationParameters);
    }

    #endregion
}
