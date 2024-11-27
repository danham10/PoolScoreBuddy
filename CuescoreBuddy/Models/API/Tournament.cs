using System.Text.Json;

namespace CuescoreBuddy.Models.API;

/// <summary>
/// Generated from special paste of JSON from https://api.cuescore.com/
/// </summary>
public partial class Tournament
{
    public int TournamentId { get; set; }
    public string? Name { get; set; }
    public string? Status { get; set; }
    public Match[]? Matches { get; set; } = [];
}

public class TournamentDecorator
{
    private List<Player>? _players;
    public Tournament Tournament { get; private set; }
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

    public async Task Fetch(IScoreAPIClient cueScoreService, int tournamentId)
    {
        Tournament = await cueScoreService.GetTournament(tournamentId);
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

    public IEnumerable<Player> GetLoadedPlayers()
    {
        var players = from p in _players
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

    public async Task<IEnumerable<Player>> GetLoadedPlayers(IScoreAPIClient cueScoreService)
    {
        if (_players == null)
            _players = await cueScoreService.GetPlayers(Tournament.TournamentId);

        return GetLoadedPlayers();
    }

    public bool IsFinished() => Tournament.Status == "Finished";
    public List<Match> PlayerMatches(int playerId)
    {
        return Tournament.Matches!.ToList().Where(m => m.PlayerA.PlayerId == playerId || m.PlayerB.PlayerId == playerId).ToList();
    }

    public List<Match> ActivePlayerMatches(int playerId)
    {
        return PlayerMatches(playerId).Where(m => m.MatchStatusCode == 1).ToList();
    }

    public List<Match> ResultsPlayerMatches(int playerId)
    {
        return PlayerMatches(playerId).Where(m => m.MatchStatusCode == 2).ToList();
    }
};





