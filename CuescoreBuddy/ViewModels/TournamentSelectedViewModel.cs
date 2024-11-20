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
    readonly DataStore _dataStore;
    readonly IMessenger _messenger;

    TournamentFacade? _tournament;

    [ObservableProperty]
    public string tournamentName;

    [ObservableProperty]
    public string errorMessage;

    //public IAsyncRelayCommand MonitorCommand { get; private set; }
    public ICommand MonitorCommand { get; private set; }
    public ICommand TournamentCommand { get; private set; }

    public TournamentSelectedViewModel()
    {
        Title = "About";

        MonitorCommand = new AsyncRelayCommand(ExecuteMonitorAsync, CanExecuteMonitor);
        TournamentCommand = new AsyncRelayCommand(ExecuteViewAsync);
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        _tournament = (query["Tournament"] as TournamentFacade);
        TournamentName = _tournament.Tournament.name;

        ErrorMessage = _tournament.IsFinished() ? "Tournament has finished" : "";
    }

    #region Commands

    private bool CanExecuteMonitor()
    {
        return string.IsNullOrEmpty(ErrorMessage);
    }

    private async Task ExecuteMonitorAsync()
    {
        var navigationParameters = new Dictionary<string, object>
        {
            { "Tournament", _tournament },
        };

        await Shell.Current.GoToAsync(nameof(PlayerPage), false, navigationParameters);
    }

    private async Task ExecuteViewAsync()
    {
        await Browser.Default.OpenAsync(_tournament.Tournament.url, BrowserLaunchMode.SystemPreferred);
    }

    #endregion
}
