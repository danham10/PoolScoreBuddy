using PoolScoreBuddy.Domain.Models.API;

namespace PoolScoreBuddy.Domain.Services;

public class TournamentService : ITournamentService
{
    public List<ITournamentDecorator> Tournaments { get; set; } = [];

    public ITournamentDecorator GetTournamentById(int tournamentId)
    {
        var tournament = Tournaments.FirstOrDefault(t => t.Tournament.TournamentId == tournamentId);

        tournament ??= new TournamentDecorator(tournamentId);

        return tournament;
    }

    public void AddIfMissing(ITournamentDecorator tournament)
    {
        var exists = (from t in Tournaments
                      where t.Tournament.TournamentId == tournament.Tournament.TournamentId
                      select t)
                      .Any();

        if (!exists)
        {
            Tournaments.Add(tournament);
        }
    }

    public void RemoveFinished() => Tournaments.ToList().RemoveAll(t => t.IsFinished());

    public bool ShouldMonitor() => ActiveTournaments() && AnyMonitoredPlayers();

    public bool ActiveTournaments()
    {
        var x = Tournaments.All(t => t.Tournament != null && t.IsFinished()) == false;
        return x;
    }

    public bool AnyMonitoredPlayers()
    {
        var x =
            (from t in Tournaments
             from mp in t.MonitoredPlayers
             select mp)
            .Any();

        return x;
    }

    public void CancelMonitoredPlayers() => Tournaments.ToList().ForEach(t => t.MonitoredPlayers = []);
}
