using PoolScoreBuddy.Domain.Models.API;

namespace PoolScoreBuddy.API.Services
{
    public interface IScoreClient
    {
        public Task<string> GetTournament(int id, string? participants, int[]? playerIds, int[]? notifiedMatchIds);
    }
}