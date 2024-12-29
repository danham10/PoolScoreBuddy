using PoolScoreBuddy.Domain.Models.API;

namespace PoolScoreBuddy.Domain.Services;

public interface IScoreAPIClient
{
    Task<Tournament> GetTournament(TournamentDto dto);
    Task<Players> GetPlayers(PlayersDto dto);
}
