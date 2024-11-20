namespace CuescoreBuddy.Models;

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

    public List<string> MonitoredPlayers()
    {
        return
            (from t in this
             from p in t.GetLoadedPlayers().Result.Where(p => p.IsMonitored)
             orderby p.MonitoredPlayer!.CreateDate descending
             select p.name
            ).ToList();
    }

    public void CancelMonitoredPlayers() => ForEach(t => t.MonitoredPlayers = []);
}
