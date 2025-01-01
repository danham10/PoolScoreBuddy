using PoolScoreBuddy.Domain.Models.API;

namespace PoolScoreBuddy.Domain.Services;

public class TournamentService : ITournamentService
{
    public List<ITournamentDecorator> Tournaments { get; set; } = [];

    public ITournamentDecorator GetTournamentById(int tournamentId)
    {
        var tournament = Tournaments.FirstOrDefault(t => t.Tournament.TournamentId == tournamentId);

        if (tournament == null)
            throw new Exception($"Tournament with id {tournamentId} not found.");

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

    public void RemoveFinished() => Tournaments.RemoveAll(t => t.IsFinished());

    public bool ShouldMonitor() => AnyActiveTournaments() && AnyMonitoredPlayers();

    public bool AnyActiveTournaments()
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

    public void CancelMonitoredPlayers() => Tournaments.ForEach(t => t.MonitoredPlayers = []);
}
