using PoolScoreBuddy.Domain.Models.API;

namespace PoolScoreBuddy.Domain.Services
{
    public interface ITournamentService
    {
        List<ITournamentDecorator> Tournaments { get; set; }
        bool ActiveTournaments();
        void AddIfMissing(ITournamentDecorator tournament);
        void RemoveFinished();
        bool AnyMonitoredPlayers();
        void CancelMonitoredPlayers();
        ITournamentDecorator GetTournamentById(int tournamentId);
        bool ShouldMonitor();
    }
}