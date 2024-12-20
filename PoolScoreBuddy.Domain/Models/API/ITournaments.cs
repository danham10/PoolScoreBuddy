
namespace PoolScoreBuddy.Domain.Models.API
{
    public interface ITournaments
    {
        bool ActiveTournaments();
        void AddIfMissing(ITournamentDecorator tournament);
        bool AnyMonitoredPlayers();
        void CancelMonitoredPlayers();
        ITournamentDecorator GetTournamentById(int tournamentId);
        bool ShouldMonitor();
    }
}