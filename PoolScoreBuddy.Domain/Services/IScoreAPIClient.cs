using PoolScoreBuddy.Domain.Models.API;

namespace PoolScoreBuddy.Domain.Services;

public interface IScoreAPIClient
{
    public Task<Tournament> GetTournament(TournamentDto dto);
    public Task<Players> GetPlayers(PlayersDto dto);
}
