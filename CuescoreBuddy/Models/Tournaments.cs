
using System.Runtime.InteropServices;

namespace CuescoreBuddy.Models;

//public record MonitoredPlayer(int PlayerId, List<int> CalledMatchIds);

public class Tournaments : List<TournamentFacade>
{
    public TournamentFacade GetTournamentById(int tournamentId)
    {
        var tournament = this.FirstOrDefault(t => t.Tournament.tournamentId == tournamentId);

        if (tournament == null)
        { 
            tournament = new TournamentFacade(tournamentId);
        }

        return tournament;
    }

    public void AddIfMissing(TournamentFacade tournament)
    {
        var exists = (from t in this
                      where t.Tournament.tournamentId == tournament.Tournament.tournamentId
                      select t).Any();

        if (!exists)
        {
            Add(tournament);
        }
    }

    //public void AddIfMissing(int tournamentId)
    //{
    //    var exists = (from t in this
    //                  where t.Tournament.tournamentId == tournamentId
    //                  select t).Any();

    //    if (!exists)
    //    {
    //        Add(new TournamentFacade(tournamentId));
    //    }
    //}

    public bool ShouldMonitor() => ActiveTournaments() && AnyMonitoredPlayers();

    public bool ActiveTournaments() {
        var x = this.All(t => t.Tournament != null && t.IsFinished()) == false;
        return x;
    }

    public bool AnyMonitoredPlayers()
    {
        var x = (
            from t in this
            from mp in t.MonitoredPlayers
            select mp)
            .Any();

        return x;
    }

    // Return all monitored players from tournament - latest first
    public List<string> MonitoredPlayers()
    {
        return
            (from t in this
             from p in t.GetLoadedPlayers().Where(p => p.IsMonitored)
             orderby p.MonitoredPlayer!.CreateDate descending
             select p.name
            ).ToList();
    }

    public void CancelMonitoredPlayers() => ForEach(t => t.MonitoredPlayers = []);

    //public async Task LoadAsync(ICueScoreService cueScoreService, int? tournamentId = null)
    //{
    //    List<int> tournamentIds = tournamentId.HasValue 
    //        ? [tournamentId.Value] 
    //        : this.Select(t => t.tournamentId).ToList();

    //    foreach (var id in tournamentIds)
    //    {
    //        var tournament = this.Where(t => t.tournamentId == id).First();
    //        tournament = await cueScoreService.GetTournament(tournament.tournamentId);
    //    }
    //}

    //public bool TogglePlayerEnabled(int playerId, int tournamentId)
    //{
    //    MonitoredPlayer? monitoredPlayer = null;

    //    var tournament = this.Where(t => t.tournamentId == tournamentId).FirstOrDefault();

    //    // If tournament not exist then add
    //    if (tournament == null)
    //    {
    //        tournament = new Tournament() { tournamentId = tournamentId};
    //        Add(tournament);
    //    }

    //    // If monitored player not exist then add
    //    monitoredPlayer = tournament.MonitoredPlayers.FirstOrDefault(p => p.PlayerId == playerId);

    //    if (monitoredPlayer == null)
    //    {
    //        monitoredPlayer = new MonitoredPlayer() { IsMonitored = false, PlayerId = playerId };
    //        tournament.MonitoredPlayers.Add(monitoredPlayer);
    //    }

    //    // Toggle monitoring
    //    monitoredPlayer.IsMonitored = !monitoredPlayer.IsMonitored;
    //    return monitoredPlayer.IsMonitored;
    //}
}

//public class MonitoredTournaments : List<MonitoredTournament>
//{
//    public async Task LoadAllAsync(ICueScoreService cueScoreService)
//    {
//        //Refresh each tournament async
//        foreach (var tournament in this)
//        {
//            tournament.Tournament = await cueScoreService.GetTournament(tournament.TournamentId);
//        }
//    }

    
//}

//public class MonitoredTournament(int tournamentId)
//{
//    public int TournamentId { get; set; } = tournamentId;
//    public Tournament? Tournament { get; set; }
//    public List<MonitoredPlayer> MonitoredPlayers { get; set; } = [];
//}




