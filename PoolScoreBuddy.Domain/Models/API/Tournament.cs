namespace PoolScoreBuddy.Domain.Models.API;

/// <summary>
/// Generated from special paste of JSON from https://api.cuescore.com/
/// Unused properties are removed to reduce memory and chance of deserialisation error
/// </summary>
public partial class Tournament
{
    public int? TournamentId { get; set; }
    public string? Name { get; set; }
    public string? Status { get; set; }
    public List<Match>? Matches { get; set; } = [];
}

public class TournamentDecorator
{
    public List<Player>? Players { get; set; }
    public Tournament Tournament { get; set; }
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
        const int MatchActiveStatus = 1;
        return PlayerMatches(playerId).Where(m => m.MatchStatusCode == MatchActiveStatus).ToList();
    }

    public List<Match> ResultsPlayerMatches(int playerId)
    {
        const int MatchFinishedStatus = 2;
        return PlayerMatches(playerId).Where(m => m.MatchStatusCode == MatchFinishedStatus).ToList();
    }
};





