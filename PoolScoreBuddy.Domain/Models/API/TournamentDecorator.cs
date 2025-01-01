namespace PoolScoreBuddy.Domain.Models.API;

public class TournamentDecorator : ITournamentDecorator
{
    public Tournament Tournament { get; set; }
    public Players? Players { get; set; }
    public List<MonitoredPlayer> MonitoredPlayers { get; set; } = [];
    public bool IsBusy { get; private set; }

    public TournamentDecorator(int tournamentId)
    {
        Tournament = new Tournament() { TournamentId = tournamentId };
    }

    public TournamentDecorator(Tournament tournament)
    {
        Tournament = tournament;
    }

    public MonitoredPlayer? TogglePlayerEnabled(int playerId)
    {
        var monitoredPlayer = MonitoredPlayers.FirstOrDefault(p => p.PlayerId == playerId);

        if (monitoredPlayer == null)
        {
            monitoredPlayer = new MonitoredPlayer() { PlayerId = playerId, CreateDate = DateTime.UtcNow };
            MonitoredPlayers.Add(monitoredPlayer);

            return monitoredPlayer;
        }
        else
        {
            MonitoredPlayers.Remove(monitoredPlayer);
            return null;
        }
    }

    public IEnumerable<Player> GetPlayersWithMonitoring()
    {
        var players = from p in Players
                      join mp in MonitoredPlayers on p.PlayerId equals mp.PlayerId
                      into MonitoredPlayersGroup
                      from mp in MonitoredPlayersGroup.DefaultIfEmpty()
                      select new Player()
                      {
                          PlayerId = p.PlayerId,
                          Firstname = p.Firstname,
                          Lastname = p.Lastname,
                          Name = p.Name,
                          Image = p.Image,
                          MonitoredPlayer = mp,
                      };

        return players.OrderBy(p => p.Name);
    }

    public bool IsFinished()
    {
        const string TournamentFinishedStatus = "Finished";
        return Tournament.Status!.Equals(TournamentFinishedStatus, StringComparison.InvariantCultureIgnoreCase);
    }
    public List<Match> PlayerMatches(int playerId)
    {
        return Tournament.Matches!.ToList().Where(m => m.PlayerA.PlayerId == playerId || m.PlayerB.PlayerId == playerId).ToList();
    }

    public List<Match> ActivePlayerMatches(int playerId)
    {
        return PlayerMatches(playerId).Where(m => m.MatchStatusCode == MatchStatusCode.Active).ToList();
    }

    public List<Match> ResultsPlayerMatches(int playerId)
    {
        return PlayerMatches(playerId).Where(m => m.MatchStatusCode == MatchStatusCode.Finished).ToList();
    }
};