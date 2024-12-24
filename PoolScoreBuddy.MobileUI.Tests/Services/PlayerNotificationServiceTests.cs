using Moq;
using PoolScoreBuddy.Domain.Models.API;
using PoolScoreBuddy.Domain.Services;
using PoolScoreBuddy.Services;
using Microsoft.Extensions.Logging;
using Plugin.LocalNotification;
using PoolScoreBuddy.Models;

namespace PoolScoreBuddy.MobileUI.Tests.Services;

public class PlayerNotificationServiceTests
{
    private readonly Mock<ITournamentService> _mockTournamentService;
    private readonly Mock<IScoreAPIClient> _mockCueScoreService;
    private readonly Mock<INotificationService> _mockNotificationService;
    private readonly Mock<ISettingsResolver> _mockSettingsResolver;
    private readonly Mock<ILogger<PlayerNotificationService>> _mockLogger;
    private readonly PlayerNotificationService _service;

    public PlayerNotificationServiceTests()
    {
        _mockTournamentService = new Mock<ITournamentService>();
        _mockCueScoreService = new Mock<IScoreAPIClient>();
        _mockNotificationService = new Mock<INotificationService>();
        _mockSettingsResolver = new Mock<ISettingsResolver>();
        _mockLogger = new Mock<ILogger<PlayerNotificationService>>();

        _service = new PlayerNotificationService(
            _mockTournamentService.Object,
            _mockCueScoreService.Object,
            _mockNotificationService.Object,
            _mockSettingsResolver.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task ProcessNotifications_HasMatch_AddsNotification()
    {
        // Arrange
        var match = new Domain.Models.API.Match
        {
            MatchId = 1,
            PlayerA = new Player { Name = "Player A" },
            PlayerB = new Player { Name = "Player B" },
            StartTime = DateTime.Now,
            StopTime = DateTime.Now,
            ScoreA = 1,
            ScoreB = 2,
            Table = new Table()
        };

        var monitoredTournament = new Tournament
        {
            TournamentId = 123,
            Matches = new List<Domain.Models.API.Match>{ match }
        };

        var tournament = new Mock<ITournamentDecorator>();
        tournament.Setup(t => t.Tournament).Returns(monitoredTournament);
        tournament.Setup(t => t.MonitoredPlayers).Returns(new List<MonitoredPlayer> { new MonitoredPlayer { PlayerId = 1 } });
        tournament.Setup(t => t.ActivePlayerMatches(It.IsAny<int>())).Returns(new List<Domain.Models.API.Match> { match });
        tournament.Setup(t => t.ResultsPlayerMatches(It.IsAny<int>())).Returns(new List<Domain.Models.API.Match>());
        
        _mockTournamentService.Setup(s => s.Tournaments).Returns(new List<ITournamentDecorator> { tournament.Object });
        _mockCueScoreService.Setup(x => x.GetTournament(It.IsAny<TournamentDto>())).ReturnsAsync(new Tournament { TournamentId = 123, Matches = new List<Domain.Models.API.Match> { new Domain.Models.API.Match { MatchId = 1, PlayerA = new Player { Name = "Player A" }, PlayerB = new Player { Name = "Player B" }, StartTime = DateTime.Now, StopTime = DateTime.Now, ScoreA = 1, ScoreB = 2 } } });
        _mockSettingsResolver.Setup(x => x.GetSettings()).Returns(new Settings());

        // Act
        var notifications = await _service.ProcessNotifications();

        // Assert
        Assert.Equal(1, notifications.Count(n => n.NotificationType == NotificationType.Start));
        Assert.Equal(0, notifications.Count(n => n.NotificationType == NotificationType.Result));
    }

    [Fact]
    public async Task ProcessNotifications_HasResult_AddsNotification()
    {
        // Arrange
        var match = new Domain.Models.API.Match
        {
            MatchId = 1,
            PlayerA = new Player { Name = "Player A" },
            PlayerB = new Player { Name = "Player B" },
            StartTime = DateTime.Now,
            StopTime = DateTime.Now,
            ScoreA = 1,
            ScoreB = 2,
            Table = new Table()
        };

        var monitoredTournament = new Tournament
        {
            TournamentId = 123,
            Matches = new List<Domain.Models.API.Match> { match }
        };

        var tournament = new Mock<ITournamentDecorator>();
        tournament.Setup(t => t.Tournament).Returns(monitoredTournament);
        tournament.Setup(t => t.MonitoredPlayers).Returns(new List<MonitoredPlayer> { new MonitoredPlayer { PlayerId = 1 } });
        tournament.Setup(t => t.ActivePlayerMatches(It.IsAny<int>())).Returns(new List<Domain.Models.API.Match>());
        tournament.Setup(t => t.ResultsPlayerMatches(It.IsAny<int>())).Returns(new List<Domain.Models.API.Match> { match });

        _mockTournamentService.Setup(s => s.Tournaments).Returns(new List<ITournamentDecorator> { tournament.Object });
        _mockCueScoreService.Setup(x => x.GetTournament(It.IsAny<TournamentDto>())).ReturnsAsync(new Tournament { TournamentId = 123, Matches = new List<Domain.Models.API.Match> { new Domain.Models.API.Match { MatchId = 1, PlayerA = new Player { Name = "Player A" }, PlayerB = new Player { Name = "Player B" }, StartTime = DateTime.Now, StopTime = DateTime.Now, ScoreA = 1, ScoreB = 2 } } });
        _mockSettingsResolver.Setup(x => x.GetSettings()).Returns(new Settings());

        // Act
        var notifications = await _service.ProcessNotifications();

        // Assert
        Assert.Equal(1, notifications.Count(n => n.NotificationType == NotificationType.Result));
        Assert.Equal(0, notifications.Count(n => n.NotificationType == NotificationType.Start));
    }


    [Fact]
    public async Task ProcessNotifications_Exception_AddsErrorNotification()
    {
        // Arrange
        _mockTournamentService.Setup(s => s.Tournaments).Throws(new Exception());
        _mockSettingsResolver.Setup(x => x.GetSettings()).Returns(new Settings());

        // Act
        var notifications = await _service.ProcessNotifications();

        // Assert
        Assert.Single(notifications);
        Assert.Equal(NotificationType.Error, notifications[0].NotificationType);
    }

    [Fact]
    public async Task SendNotifications_ValidNotifications_SendsNotifications()
    {
        // Arrange
        var notifications = new List<CuescoreNotification>
        {
            new CuescoreNotification(NotificationType.Start, 1, "Player A", "Player B", DateTime.Now, "Table 1"),
            new CuescoreNotification(NotificationType.Result, 2, "Player A", "Player B", DateTime.Now, "1 - 2")
        };

        // Act
        await _service.SendNotifications(notifications);

        // Assert
        _mockNotificationService.Verify(x => x.Show(It.IsAny<NotificationRequest>()), Times.Exactly(notifications.Count));
    }

    [Fact]
    public async Task SendNotifications_InvalidNotificationType_ThrowsException()
    {
        // Arrange
        var notifications = new List<CuescoreNotification>
        {
            new CuescoreNotification((NotificationType)999, 1, "Player A", "Player B", DateTime.Now, "Invalid")
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidDataException>(() => _service.SendNotifications(notifications));
    }
}
