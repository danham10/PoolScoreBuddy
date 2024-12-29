
namespace PoolScoreBuddy.Domain.Models.API;

public interface ITournamentDecorator
{
    bool IsBusy { get; }
    List<MonitoredPlayer> MonitoredPlayers { get; set; }
    Players? Players { get; set; }
    Tournament Tournament { get; set; }

    List<Match> ActivePlayerMatches(int playerId);
    IEnumerable<Player> GetPlayersWithMonitoring();
    bool IsFinished();
    List<Match> PlayerMatches(int playerId);
    List<Match> ResultsPlayerMatches(int playerId);
    MonitoredPlayer? TogglePlayerEnabled(int playerId);
}