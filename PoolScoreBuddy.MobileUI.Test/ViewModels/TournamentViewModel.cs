using Moq;
using PoolScoreBuddy.Domain.Models;
using PoolScoreBuddy.Domain.Models.API;
using PoolScoreBuddy.Domain.Services;
using PoolScoreBuddy.ViewModels;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using PoolScoreBuddy.Services;
using PoolScoreBuddy.Views;

namespace PoolScoreBuddy.MobileUI.Test.ViewModels
{
    public class TournamentViewModelTests
    {
        private readonly Mock<IScoreAPIClient> _mockScoreAPIClient;
        private readonly Mock<ITournamentService> _mockTournamentService;
        private readonly Mock<IEnsureConnectivity> _mockEnsureConnectivity;
        private readonly Mock<IAlert> _mockAlert;
        private readonly Mock<IPoolAppShell> _mockAppShell;
        private readonly Mock<ISettingsResolver> _mockSettingsResolver;
        private readonly Mock<ILogger<TournamentViewModel>> _mockLogger;
        private readonly TournamentViewModel _viewModel;

        public TournamentViewModelTests()
        {
            _mockScoreAPIClient = new Mock<IScoreAPIClient>();
            _mockTournamentService = new Mock<ITournamentService>();
            _mockEnsureConnectivity = new Mock<IEnsureConnectivity>();
            _mockAlert = new Mock<IAlert>();
            _mockAppShell = new Mock<IPoolAppShell>();
            _mockSettingsResolver = new Mock<ISettingsResolver>();
            _mockLogger = new Mock<ILogger<TournamentViewModel>>();

            _viewModel = new TournamentViewModel(
                _mockScoreAPIClient.Object,
                _mockTournamentService.Object,
                _mockEnsureConnectivity.Object,
                _mockAlert.Object,
                _mockAppShell.Object,
                _mockSettingsResolver.Object,
                _mockLogger.Object);
        }

        [Fact]
        public void CanExecuteTournamentLoad_ValidTournamentId_ReturnsTrue()
        {
            // Arrange
            _viewModel.TournamentId = "123";

            // Act
            var result = _viewModel.CanExecuteTournamentLoad();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void CanExecuteTournamentLoad_InvalidTournamentId_ReturnsFalse()
        {
            // Arrange
            _viewModel.TournamentId = "";

            // Act
            var result = _viewModel.CanExecuteTournamentLoad();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task TournamentLoad_ValidTournamentId_LoadsTournament()
        {
            // Arrange
            _viewModel.TournamentId = "123";
            _mockEnsureConnectivity.Setup(x => x.IsConnectedWithAlert()).ReturnsAsync(true);
            _mockScoreAPIClient.Setup(x => x.GetTournament(It.IsAny<TournamentDto>())).ReturnsAsync(new Tournament());
            _mockSettingsResolver.Setup(x => x.GetSettings()).Returns(new Settings());

            // Act
            await _viewModel.TournamentLoad();

            // Assert
            _mockScoreAPIClient.Verify(x => x.GetTournament(It.IsAny<TournamentDto>()), Times.Once);
        }

        [Fact]
        public async Task TournamentLoad_InvalidTournamentId_DoesNotLoadTournament()
        {
            // Arrange
            _viewModel.TournamentId = "";
            _mockEnsureConnectivity.Setup(x => x.IsConnectedWithAlert()).ReturnsAsync(true);

            // Act
            await _viewModel.TournamentLoad();

            // Assert
            _mockScoreAPIClient.Verify(x => x.GetTournament(It.IsAny<TournamentDto>()), Times.Never);
        }

        [Fact]
        public async Task TournamentLoad_HttpRequestException_ShowsAlert()
        {
            // Arrange
            _viewModel.TournamentId = "123";
            _mockEnsureConnectivity.Setup(x => x.IsConnectedWithAlert()).ReturnsAsync(true);
            _mockScoreAPIClient.Setup(x => x.GetTournament(It.IsAny<TournamentDto>())).ThrowsAsync(new HttpRequestException());

            // Act
            await _viewModel.TournamentLoad();

            // Assert
            _mockAlert.Verify(x => x.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task TournamentLoad_APIServerException_ShowsAlert()
        {
            // Arrange
            _viewModel.TournamentId = "123";
            _mockEnsureConnectivity.Setup(x => x.IsConnectedWithAlert()).ReturnsAsync(true);
            _mockScoreAPIClient.Setup(x => x.GetTournament(It.IsAny<TournamentDto>())).ThrowsAsync(new APIServerException("Server error"));

            // Act
            await _viewModel.TournamentLoad();

            // Assert
            _mockAlert.Verify(x => x.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task TournamentLoad_JsonException_ShowsAlert()
        {
            // Arrange
            _viewModel.TournamentId = "123";
            _mockEnsureConnectivity.Setup(x => x.IsConnectedWithAlert()).ReturnsAsync(true);
            _mockScoreAPIClient.Setup(x => x.GetTournament(It.IsAny<TournamentDto>())).ThrowsAsync(new JsonException());

            // Act
            await _viewModel.TournamentLoad();

            // Assert
            _mockAlert.Verify(x => x.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task GetTournament_ValidTournamentId_ReturnsTournament()
        {
            // Arrange
            _viewModel.TournamentId = "123";
            var tournament = new Tournament();
            _mockScoreAPIClient.Setup(x => x.GetTournament(It.IsAny<TournamentDto>())).ReturnsAsync(tournament);
            _mockSettingsResolver.Setup(x => x.GetSettings()).Returns(new Settings());

            // Act
            var result = await _viewModel.GetTournament();

            // Assert
            Assert.Equal(tournament, result);
        }

        [Fact]
        public async Task GoToTournamentSelectedPage_NavigatesToTournamentSelectedPage()
        {
            // Arrange
            var tournament = new TournamentDecorator(new Tournament());

            // Act
            await _viewModel.GoToTournamentSelectedPage(tournament);

            // Assert
            _mockAppShell.Verify(x => x.GoToAsync(nameof(TournamentSelectedPage), It.IsAny<Dictionary<string, object>>()), Times.Once);
        }
    }
}