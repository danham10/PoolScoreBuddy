﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PoolScoreBuddy.Domain.Models.API;
using PoolScoreBuddy.Resources;

namespace PoolScoreBuddy.ViewModels;
public partial class TournamentSelectedViewModel(IDataStore dataStore, IPoolAppShell appShell) : BaseViewModel, IQueryAttributable
{
    ITournamentDecorator? _tournament;

    [ObservableProperty]
    public string tournamentName = "";

    [ObservableProperty]
    public string message = "";

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        _tournament = dataStore.Tournaments.GetTournamentById(Convert.ToInt32(query["TournamentId"]));

        TournamentName = _tournament.Tournament.Name ?? AppResources.TournamentNoName;

        Message = _tournament!.IsFinished() ?
            string.Format(AppResources.TournamentFinishedLabel, TournamentName) :
            string.Format(AppResources.TournamentSelectedLabel, TournamentName);
    }

    private bool CanExecuteMonitor()
    {
        return _tournament != null &&_tournament!.IsFinished() == false;
    }

    [RelayCommand(CanExecute = nameof(CanExecuteMonitor))]
    private async Task Monitor()
    {
        var navigationParameters = new Dictionary<string, object>
        {
            { "TournamentId", _tournament!.Tournament.TournamentId! },
        };

        await appShell.GoToAsync(nameof(PlayerPage), navigationParameters);
    }
}
