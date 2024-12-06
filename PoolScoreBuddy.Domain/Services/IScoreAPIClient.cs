using PoolScoreBuddy.Di;
using PoolScoreBuddy.Domain.Models.API;

namespace PoolScoreBuddy.Domain.Services;

public interface IScoreAPIClient
{
    public Task<Tournament> GetTournament(string baseUrl, int tournamentId, IEnumerable<int>? playerIds = null);
    public Task<List<Player>> GetPlayers(string baseUrl, int tournamentId);
}
