﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Maui.Core.Platform;

namespace CuescoreBuddy.ViewModels;
public partial class TournamentViewModel : BaseViewModel
{
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(TournamentLoadCommand))]
    private string? tournamentId;

    [ObservableProperty]
    private string? errorMessage;

    public TournamentViewModel()
    {
        Title = "About";
    }

    private bool CanExecuteTournamentLoad() => !string.IsNullOrEmpty(TournamentId);

    [RelayCommand(CanExecute = nameof(CanExecuteTournamentLoad))]
    private async Task TournamentLoad()
    {
        try
        {
            IsBusy = true;

            var cueScoreService = ServiceResolver.GetService<ICueScoreService>();
            Tournament? tournament = await cueScoreService.GetTournament(Convert.ToInt32(TournamentId));

            if (tournament != null)
            {
                TournamentFacade tournamentFacade = new(tournament);

                IsBusy = false;
                await GoToTournamentSelectedPage(tournamentFacade);
            }
        }
        catch (Exception)
        {
            IsBusy = false;
            await Application.Current.MainPage.DisplayAlert("Cannot load tournament", "Check tournament number and network connectivity", "OK");
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
}
