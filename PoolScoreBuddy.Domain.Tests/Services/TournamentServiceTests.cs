using Moq;
using PoolScoreBuddy.Domain.Models.API;
using PoolScoreBuddy.Domain.Services;

namespace PoolScoreBuddy.Domain.Tests.Services
{
    public class TournamentServiceTests
    {
        private readonly TournamentService _tournamentService;

        public TournamentServiceTests()
        {
            _tournamentService = new TournamentService();
        }

        [Fact]
        public void AddIfMissing_ValidTournament_AddsTournament()
        {
            // Arrange
            var tournament = new Mock<ITournamentDecorator>();
            tournament.Setup(t => t.Tournament).Returns(new Tournament { TournamentId = 123 });

            // Act
            _tournamentService.AddIfMissing(tournament.Object);

            // Assert
            Assert.Single(_tournamentService.Tournaments);
            Assert.Equal(123, _tournamentService.Tournaments.First().Tournament.TournamentId);
        }

        [Fact]
        public void AddIfMissing_DuplicateTournament_DoesNotAddIfMissing()
        {
            // Arrange
            var tournament = new Mock<ITournamentDecorator>();
            tournament.Setup(t => t.Tournament).Returns(new Tournament { TournamentId = 123 });

            // Act
            _tournamentService.AddIfMissing(tournament.Object);
            _tournamentService.AddIfMissing(tournament.Object);

            // Assert
            Assert.Single(_tournamentService.Tournaments);
        }

        [Fact]
        public void GetTournamentById_ValidId_ReturnsTournament()
        {
            // Arrange
            var tournament = new Mock<ITournamentDecorator>();
            tournament.Setup(t => t.Tournament).Returns(new Tournament { TournamentId = 123 });

            _tournamentService.AddIfMissing(tournament.Object);

            // Act
            var result = _tournamentService.GetTournamentById(123);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(123, result.Tournament.TournamentId);
        }

        [Fact]
        public void GetTournamentById_InvalidId_ThrowsException()
        {
            // Assert
            Assert.Throws<Exception>(() => _tournamentService.GetTournamentById(123));
        }

        [Fact]
        public void RemoveFinished_RemovesFinishedTournaments()
        {
            // Arrange
            var finishedTournament = new Mock<ITournamentDecorator>();
            finishedTournament.Setup(t => t.Tournament).Returns(new Tournament() { TournamentId = 123 });
            finishedTournament.Setup(t => t.IsFinished()).Returns(true);

            var activeTournament = new Mock<ITournamentDecorator>();
            activeTournament.Setup(t => t.Tournament).Returns(new Tournament() { TournamentId = 456 });
            activeTournament.Setup(t => t.IsFinished()).Returns(false);

            _tournamentService.AddIfMissing(finishedTournament.Object);
            _tournamentService.AddIfMissing(activeTournament.Object);

            // Act
            _tournamentService.RemoveFinished();

            // Assert
            Assert.Single(_tournamentService.Tournaments);
            Assert.Equal(456, _tournamentService.Tournaments.First().Tournament.TournamentId);
        }

        [Fact]
        public void CancelMonitoredPlayers_CancelsMonitoredPlayers()
        {
            // Arrange
            var monitoredPlayers = new List<MonitoredPlayer>() { new() };

            var tournament = new TournamentDecorator(123) { MonitoredPlayers = monitoredPlayers};

            _tournamentService.AddIfMissing(tournament);

            // Act
            _tournamentService.CancelMonitoredPlayers();

            // Assert
            Assert.Empty(_tournamentService.Tournaments.First().MonitoredPlayers);
        }

        [Fact]
        public void AnyMonitoredPlayers_AnyMonitoredPlayers_ReturnsTrue()
        {
            // Arrange
            var monitoredPlayers = new List<MonitoredPlayer>() { new() };

            var tournament = new TournamentDecorator(123) { MonitoredPlayers = monitoredPlayers };

            _tournamentService.AddIfMissing(tournament);

            // Act
            var result = _tournamentService.AnyMonitoredPlayers();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void AnyMonitoredPlayers_NoMonitoredPlayers_ReturnsFalse()
        {
            // Arrange
            var tournament = new TournamentDecorator(123);

            _tournamentService.AddIfMissing(tournament);

            // Act
            var result = _tournamentService.AnyMonitoredPlayers();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void ActiveTournaments_AnyActiveTournaments_ReturnsTrue()
        {
            // Arrange

            var activeTournament = new Mock<ITournamentDecorator>();
            activeTournament.Setup(t => t.IsFinished()).Returns(false);

            _tournamentService.AddIfMissing(activeTournament.Object);

            // Act
            var result = _tournamentService.AnyActiveTournaments();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void ActiveTournaments_NoActiveTournaments_ReturnsFalse()
        {
            // Arrange
            var activeTournament = new Mock<ITournamentDecorator>();
            activeTournament.Setup(t => t.Tournament).Returns(new Tournament());
            activeTournament.Setup(t => t.IsFinished()).Returns(true);

            _tournamentService.AddIfMissing(activeTournament.Object);

            // Act
            var result = _tournamentService.AnyActiveTournaments();

            // Assert
            Assert.False(result);
        }


        [Theory]
        [InlineData(true, true, true)]
        [InlineData(false, true, false)]
        [InlineData(true, false, false)]
        public void ShouldMonitor_ReturnsCorrectBool(bool anyActiveTournaments, bool anyMonitoredPlayers, bool shouldMonitor)
        {
            // Arrange
            var monitoredPlayers = anyMonitoredPlayers ? new List<MonitoredPlayer>() { new() } : [];

            var tournament = new TournamentDecorator(123) { MonitoredPlayers = monitoredPlayers};
            tournament.Tournament.Status = anyActiveTournaments ? "active" : "finished";

            _tournamentService.AddIfMissing(tournament);

            // Act
            var result = _tournamentService.ShouldMonitor();

            // Assert
            Assert.Equal(shouldMonitor, result);
        }

    }
}