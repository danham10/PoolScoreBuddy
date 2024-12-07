using PoolScoreBuddy.Domain.Models.API;

namespace PoolScoreBuddy.Domain.Services;

public interface IScoreAPIClient
{
    public Task<Tournament> GetTournament(TournamentDto dto);
    public Task<List<Player>> GetPlayers(PlayersDto dto);
}
