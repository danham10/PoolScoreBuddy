using PoolScoreBuddy.Domain.Models.API;

namespace PoolScoreBuddy.Domain.Services;

public interface IScoreAPIClient
{
    Task<Tournament> GetTournament(ApiDto dto);
    Task<Players> GetPlayers(ApiDto dto);
}
