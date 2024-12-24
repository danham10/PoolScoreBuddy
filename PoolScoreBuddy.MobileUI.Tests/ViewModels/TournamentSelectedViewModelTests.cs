using Moq;
using PoolScoreBuddy.Domain.Services;
using PoolScoreBuddy.ViewModels;
using PoolScoreBuddy.Resources;
using PoolScoreBuddy.Domain.Models.API;
using PoolScoreBuddy.Services;
using PoolScoreBuddy.Views;

namespace PoolScoreBuddy.MobileUI.Test.ViewModels
{
    public class TournamentSelectedViewModelTests
    {
        private readonly Mock<ITournamentService> _mockTournamentService;
        private readonly Mock<IPoolAppShell> _mockAppShell;
        private readonly TournamentSelectedViewModel _viewModel;

        public TournamentSelectedViewModelTests()
        {
            _mockTournamentService = new Mock<ITournamentService>();
            _mockAppShell = new Mock<IPoolAppShell>();

            _viewModel = new TournamentSelectedViewModel(
                _mockTournamentService.Object,
                _mockAppShell.Object);
        }

        [Fact]
        public void ApplyQueryAttributes_ValidTournamentId_SetsProperties()
        {
            // Arrange
            var tournament = new Mock<ITournamentDecorator>();
            tournament.Setup(t => t.Tournament).Returns(new Tournament { TournamentId = 123, Name = "Test Tournament" });
            tournament.Setup(t => t.IsFinished()).Returns(false);

            _mockTournamentService.Setup(s => s.GetTournamentById(123)).Returns(tournament.Object);

            var query = new Dictionary<string, object>
            {
                { "TournamentId", 123 }
            };

            // Act
            _viewModel.ApplyQueryAttributes(query);

            // Assert
            Assert.Equal("Test Tournament", _viewModel.TournamentName);
            Assert.Equal(string.Format(AppResources.TournamentSelectedLabel, "Test Tournament"), _viewModel.Message);
        }

        [Fact]
        public void ApplyQueryAttributes_TournamentFinished_SetsFinishedMessage()
        {
            // Arrange
            var tournament = new Mock<ITournamentDecorator>();
            tournament.Setup(t => t.Tournament).Returns(new Tournament { TournamentId = 123, Name = "Test Tournament" });
            tournament.Setup(t => t.IsFinished()).Returns(true);

            _mockTournamentService.Setup(s => s.GetTournamentById(123)).Returns(tournament.Object);

            var query = new Dictionary<string, object>
            {
                { "TournamentId", 123 }
            };

            // Act
            _viewModel.ApplyQueryAttributes(query);

            // Assert
            Assert.Equal("Test Tournament", _viewModel.TournamentName);
            Assert.Equal(string.Format(AppResources.TournamentFinishedLabel, "Test Tournament"), _viewModel.Message);
        }

        [Fact]
        public void CanExecuteMonitor_TournamentNotFinished_ReturnsTrue()
        {
            // Arrange
            var tournament = new Mock<ITournamentDecorator>();
            tournament.Setup(t => t.IsFinished()).Returns(false);
            _viewModel.GetType().GetField("_tournament", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!.SetValue(_viewModel, tournament.Object);

            // Act
            var result = _viewModel.CanExecuteMonitor();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void CanExecuteMonitor_TournamentFinished_ReturnsFalse()
        {
            // Arrange
            var tournament = new Mock<ITournamentDecorator>();
            tournament.Setup(t => t.IsFinished()).Returns(true);
            _viewModel.GetType().GetField("_tournament", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!.SetValue(_viewModel, tournament.Object);

            // Act
            var result = _viewModel.CanExecuteMonitor();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task Monitor_NavigatesToPlayerPage()
        {
            // Arrange
            var tournament = new Mock<ITournamentDecorator>();
            tournament.Setup(t => t.Tournament).Returns(new Tournament { TournamentId = 123 });
            _viewModel.GetType().GetField("_tournament", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!.SetValue(_viewModel, tournament.Object);

            // Act
            await _viewModel.Monitor();

            // Assert
            _mockAppShell.Verify(x => x.GoToAsync(nameof(PlayerPage), It.IsAny<Dictionary<string, object>>()), Times.Once);
        }
    }
}