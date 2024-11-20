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

    int _tournamentId;
    TournamentFacade? _tournament;

    [ObservableProperty]
    public string tournamentName;

    //public IAsyncRelayCommand MonitorCommand { get; private set; }
    public ICommand MonitorCommand { get; private set; }
    public ICommand TournamentCommand { get; private set; }

    public TournamentSelectedViewModel()
    {
        Title = "About";
        Description = "51123145";  //50522059

        MonitorCommand = new AsyncRelayCommand(ExecuteMonitorAsync);
        //TournamentCommand = new AsyncRelayCommand(ExecuteView);

        //TODO On activating app paste in clipboard (if tournament) to textbox
        //MonitorCommand = new AsyncRelayCommand(ExecuteMonitor);
        //MonitorCommand = new Command(ExecuteMonitor, CanExecuteMonitor);
        //PropertyChanged += (_, __) => MonitorCommand.ChangeCanExecute();


        //ViewCommand = new Command(ExecuteView);
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        _tournament = (query["Tournament"] as TournamentFacade);
        TournamentName = _tournament.Tournament.name;

        //query.Clear();
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

    private bool CanExecuteMonitor()
    {
        return !_tournament.IsFinished();
    }

    private async Task ExecuteMonitorAsync()
    {
        var navigationParameters = new Dictionary<string, object>
        {
            { "Tournament", _tournament },
        };

        await Shell.Current.GoToAsync(nameof(PlayerPage), false, navigationParameters);
    }

    private async Task ExecuteView()
    {
        await Browser.Default.OpenAsync(_tournament.Tournament.url, BrowserLaunchMode.SystemPreferred);
    }

    #endregion
}
