using PoolScoreBuddy.Domain.Models.API;

namespace PoolScoreBuddy.Domain.Services;

public interface IScoreAPIClient
{
    public Task<Tournament> GetTournament(string baseUrl, int tournamentId, int? playerId = null);
    public Task<List<Player>> GetPlayers(string baseUrl, int tournamentId);
}
