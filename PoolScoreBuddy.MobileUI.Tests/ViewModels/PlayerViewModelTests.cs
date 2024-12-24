using Moq;
using PoolScoreBuddy.Domain.Models;
using PoolScoreBuddy.Domain.Models.API;
using PoolScoreBuddy.Domain.Services;
using PoolScoreBuddy.ViewModels;
using Microsoft.Extensions.Logging;
using CommunityToolkit.Mvvm.Messaging;
using System.Text.Json;
using PoolScoreBuddy.Services;
using PoolScoreBuddy.Domain;
using System.Collections.ObjectModel;

namespace PoolScoreBuddy.MobileUI.Test.ViewModels;

public class PlayerViewModelTests
{
    private readonly Mock<ITournamentService> _mockTournamentService;
    private readonly Mock<IMessenger> _mockMessenger;
    private readonly Mock<IScoreAPIClient> _mockCueScoreService;
    private readonly Mock<IEnsureConnectivity> _mockEnsureConnectivity;
    private readonly Mock<ISettingsResolver> _mockSettingsResolver;
    private readonly Mock<INotificationsChallenger> _mockNotificationsChallenger;
    private readonly Mock<IAlert> _mockAlert;
    private readonly Mock<ILogger<PlayerViewModel>> _mockLogger;
    private readonly PlayerViewModel _viewModel;

    public PlayerViewModelTests()
    {
        _mockTournamentService = new Mock<ITournamentService>();
        _mockMessenger = new Mock<IMessenger>();
        _mockCueScoreService = new Mock<IScoreAPIClient>();
        _mockEnsureConnectivity = new Mock<IEnsureConnectivity>();
        _mockSettingsResolver = new Mock<ISettingsResolver>();
        _mockNotificationsChallenger = new Mock<INotificationsChallenger>();
        _mockAlert = new Mock<IAlert>();
        _mockLogger = new Mock<ILogger<PlayerViewModel>>();

        _viewModel = new PlayerViewModel(
            _mockTournamentService.Object,
            _mockMessenger.Object,
            _mockCueScoreService.Object,
            _mockEnsureConnectivity.Object,
            _mockSettingsResolver.Object,
            _mockNotificationsChallenger.Object,
            _mockAlert.Object,
            _mockLogger.Object);
    }

    [Fact]
    public void ApplyQueryAttributes_ValidTournamentId_SetsTournament()
    {
        // Arrange
        var tournament = new Mock<ITournamentDecorator>();
        tournament.Setup(t => t.Tournament).Returns(new Tournament { TournamentId = 123 });

        _mockTournamentService.Setup(s => s.GetTournamentById(123)).Returns(tournament.Object);

        var query = new Dictionary<string, object>
        {
            { "TournamentId", 123 }
        };

        // Act
        _viewModel.ApplyQueryAttributes(query);

        // Assert
        Assert.Equal(tournament.Object, _viewModel.GetType().GetField("_tournament", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!.GetValue(_viewModel));
    }

    [Fact]
    public async Task Refresh_ValidTournamentId_RefreshesPlayers()
    {
        // Arrange
        var tournament = new Mock<ITournamentDecorator>();
        tournament.Setup(t => t.Tournament).Returns(new Tournament { TournamentId = 123 });
        tournament.Setup(t => t.GetPlayersWithMonitoring()).Returns(new List<Player> { new Player { PlayerId = 1, Name = "Player 1" } });

        _mockTournamentService.Setup(s => s.GetTournamentById(123)).Returns(tournament.Object);
        _mockEnsureConnectivity.Setup(x => x.IsConnectedWithAlert()).ReturnsAsync(true);
        _mockCueScoreService.Setup(x => x.GetPlayers(It.IsAny<PlayersDto>())).ReturnsAsync(new List<Player> { new Player { PlayerId = 1, Name = "Player 1" } });
        _mockSettingsResolver.Setup(x => x.GetSettings()).Returns(new Settings());

        _viewModel.ApplyQueryAttributes(new Dictionary<string, object> { { "TournamentId", 123 } });

        // Act
        await _viewModel.Refresh();

        // Assert
        Assert.Single(_viewModel.Players);
        Assert.Equal("Player 1", _viewModel.Players[0].Name);
    }

    [Fact]
    public async Task Refresh_HttpRequestException_ShowsAlert()
    {
        // Arrange
        var tournament = new Mock<ITournamentDecorator>();
        tournament.Setup(t => t.Tournament).Returns(new Tournament { TournamentId = 123 });

        _mockTournamentService.Setup(s => s.GetTournamentById(123)).Returns(tournament.Object);
        _mockEnsureConnectivity.Setup(x => x.IsConnectedWithAlert()).ReturnsAsync(true);
        _mockCueScoreService.Setup(x => x.GetPlayers(It.IsAny<PlayersDto>())).ThrowsAsync(new HttpRequestException());

        _viewModel.ApplyQueryAttributes(new Dictionary<string, object> { { "TournamentId", 123 } });

        // Act
        await _viewModel.Refresh();

        // Assert
        _mockAlert.Verify(x => x.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task Refresh_APIServerException_ShowsAlert()
    {
        // Arrange
        var tournament = new Mock<ITournamentDecorator>();
        tournament.Setup(t => t.Tournament).Returns(new Tournament { TournamentId = 123 });

        _mockTournamentService.Setup(s => s.GetTournamentById(123)).Returns(tournament.Object);
        _mockEnsureConnectivity.Setup(x => x.IsConnectedWithAlert()).ReturnsAsync(true);
        _mockCueScoreService.Setup(x => x.GetPlayers(It.IsAny<PlayersDto>())).ThrowsAsync(new APIServerException("Server error"));

        _viewModel.ApplyQueryAttributes(new Dictionary<string, object> { { "TournamentId", 123 } });

        // Act
        await _viewModel.Refresh();

        // Assert
        _mockAlert.Verify(x => x.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task Refresh_JsonException_ShowsAlert()
    {
        // Arrange
        var tournament = new Mock<ITournamentDecorator>();
        tournament.Setup(t => t.Tournament).Returns(new Tournament { TournamentId = 123 });

        _mockTournamentService.Setup(s => s.GetTournamentById(123)).Returns(tournament.Object);
        _mockEnsureConnectivity.Setup(x => x.IsConnectedWithAlert()).ReturnsAsync(true);
        _mockCueScoreService.Setup(x => x.GetPlayers(It.IsAny<PlayersDto>())).ThrowsAsync(new JsonException());

        _viewModel.ApplyQueryAttributes(new Dictionary<string, object> { { "TournamentId", 123 } });

        // Act
        await _viewModel.Refresh();

        // Assert
        _mockAlert.Verify(x => x.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task Appearing_CallsRefresh()
    {
        // Arrange
        var tournament = new Mock<ITournamentDecorator>();
        tournament.Setup(t => t.Tournament).Returns(new Tournament { TournamentId = 123 });
        tournament.Setup(t => t.GetPlayersWithMonitoring()).Returns(new List<Player> { new Player { PlayerId = 1, Name = "Player 1" } });

        _mockTournamentService.Setup(s => s.GetTournamentById(123)).Returns(tournament.Object);
        _mockEnsureConnectivity.Setup(x => x.IsConnectedWithAlert()).ReturnsAsync(true);
        _mockCueScoreService.Setup(x => x.GetPlayers(It.IsAny<PlayersDto>())).ReturnsAsync(new List<Player> { new Player { PlayerId = 1, Name = "Player 1" } });
        _mockSettingsResolver.Setup(x => x.GetSettings()).Returns(new Settings());

        _viewModel.ApplyQueryAttributes(new Dictionary<string, object> { { "TournamentId", 123 } });

        // Act
        await _viewModel.Appearing();

        // Assert
        Assert.Single(_viewModel.Players);
        Assert.Equal("Player 1", _viewModel.Players[0].Name);
    }

    [Fact]
    public void ToggleStartMonitor_MaximumMonitorCountReached_ShowsAlert()
    {
        // Arrange
        var tournament = new Mock<ITournamentDecorator>();
        tournament.Setup(t => t.Tournament).Returns(new Tournament { TournamentId = 123 });

        var maximumMonitoredPlayers = new List<MonitoredPlayer>();
        for (int i = 1; i <= Constants.MaximumMonitoredPlayersPerTournament; i++)
        {
            maximumMonitoredPlayers.Add(new MonitoredPlayer { PlayerId = i });
        }
        tournament.Setup(t => t.MonitoredPlayers).Returns(maximumMonitoredPlayers);

        _mockTournamentService.Setup(s => s.GetTournamentById(123)).Returns(tournament.Object);

        _viewModel.ApplyQueryAttributes(new Dictionary<string, object> { { "TournamentId", 123 } });

        // Act
        _viewModel.ToggleStartMonitor.Execute(new Player { PlayerId = 1, Name = "Player 1" });

        // Assert
        _mockAlert.Verify(x => x.Show(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public void ToggleStartMonitor_NotificationsNotAllowed_DoesNotTogglePlayer()
    {
        // Arrange
        var tournament = new Mock<ITournamentDecorator>();
        tournament.Setup(t => t.Tournament).Returns(new Tournament { TournamentId = 123 });
        tournament.Setup(t => t.MonitoredPlayers).Returns(new List<MonitoredPlayer>());

        _mockTournamentService.Setup(s => s.GetTournamentById(123)).Returns(tournament.Object);
        _mockNotificationsChallenger.Setup(x => x.AllowNotificationsAsync()).ReturnsAsync(false);

        _viewModel.ApplyQueryAttributes(new Dictionary<string, object> { { "TournamentId", 123 } });

        // Act
        _viewModel.ToggleStartMonitor.Execute(new Player { PlayerId = 1, Name = "Player 1" });

        // Assert
        tournament.Verify(t => t.TogglePlayerEnabled(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public void ToggleStartMonitor_ValidPlayer_TogglesPlayer()
    {
        // Arrange
        var tournament = new Mock<ITournamentDecorator>();
        tournament.Setup(t => t.Tournament).Returns(new Tournament { TournamentId = 123 });
        tournament.Setup(t => t.MonitoredPlayers).Returns(new List<MonitoredPlayer>());
        tournament.Setup(t => t.TogglePlayerEnabled(It.IsAny<int>())).Returns(new MonitoredPlayer());

        _mockTournamentService.Setup(s => s.GetTournamentById(123)).Returns(tournament.Object);
        _mockNotificationsChallenger.Setup(x => x.AllowNotificationsAsync()).ReturnsAsync(true);

        _viewModel.ApplyQueryAttributes(new Dictionary<string, object> { { "TournamentId", 123 } });
        _viewModel.Players = new ObservableCollection<Player> { new Player { PlayerId = 1, Name = "Player 1" } };

        // Act
        _viewModel.ToggleStartMonitor.Execute(new Player { PlayerId = 1, Name = "Player 1" });

        // Assert
        tournament.Verify(t => t.TogglePlayerEnabled(1), Times.Once);
    }
}
