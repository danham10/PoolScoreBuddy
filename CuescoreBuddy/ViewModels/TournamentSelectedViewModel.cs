using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CuescoreBuddy.Models;
using CuescoreBuddy.Services;
using CuescoreBuddy.Views;
using Microsoft.Maui.ApplicationModel;
using System;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace CuescoreBuddy.ViewModels;
public partial class TournamentSelectedViewModel : BaseViewModel, IQueryAttributable
{
    TournamentDecorator? _tournament;

    [ObservableProperty]
    public required string tournamentName;

    [ObservableProperty]
    public string errorMessage = "";

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
    }

    #region Commands

    private bool CanExecuteMonitor()
    {
        ErrorMessage = _tournament!.IsFinished() ? "Tournament has finished" : "";
        return string.IsNullOrEmpty(ErrorMessage);
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
