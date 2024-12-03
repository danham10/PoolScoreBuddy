using PoolScoreBuddy.Domain.Models.API;

namespace PoolScoreBuddy.API.Services
{
    public interface IScoreClient
    {
        Task<Tournament> GetTournament(int id, int? playerId, int[]? notifiedMatchIds);
    }
}