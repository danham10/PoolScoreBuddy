using PoolScoreBuddy.Models.API;

namespace PoolScoreBuddy.Services;

public interface IScoreAPIClient
{
    public Task<Tournament> GetTournament(int tournamentId);
    public Task<List<Player>> GetPlayers(int tournamentId);
}
