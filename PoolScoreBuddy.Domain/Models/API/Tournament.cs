using PoolScoreBuddy.Domain.Services;

namespace PoolScoreBuddy.Domain.Models.API;

/// <summary>
/// Generated from special paste of JSON from https://api.cuescore.com/
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

    public async Task Fetch(IScoreAPIClient cueScoreService, string baseUrl, int tournamentId, IEnumerable<int>? playerIds = null)
    {
        TournamentDto dto = new()
        {
            BaseUrl = baseUrl,
            TournamentId = tournamentId,
            PlayerIds = playerIds
        };

        Tournament = await cueScoreService.GetTournament(dto);
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

    public IEnumerable<Player> GetPlayers()
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

    public async Task<IEnumerable<Player>> GetPlayers(IScoreAPIClient cueScoreService, string APIBaseUrl)
    {
        PlayersDto dto = new()
        {
            BaseUrl = APIBaseUrl,
            TournamentId = Tournament.TournamentId!.Value,
        };

        _players ??= await cueScoreService.GetPlayers(dto);

        return GetPlayers();
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





