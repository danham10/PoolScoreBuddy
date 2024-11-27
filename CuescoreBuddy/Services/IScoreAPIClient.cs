using CuescoreBuddy.Models.API;

namespace CuescoreBuddy.Services
{
    public interface IScoreAPIClient
    {
        public Task<Tournament> GetTournament(int tournamentId);
        public Task<List<Player>> GetPlayers(int tournamentId);
    }
}
