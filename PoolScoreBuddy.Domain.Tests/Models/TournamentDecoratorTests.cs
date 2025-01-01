using PoolScoreBuddy.Domain.Models.API;
using Match = PoolScoreBuddy.Domain.Models.API.Match;

namespace PoolScoreBuddy.Domain.Tests.Models;

public class TournamentDecoratorTests
{
    [Fact]
    public void Constructor_WithTournamentId_SetsTournamentId()
    {
        // Arrange
        int tournamentId = 123;

        // Act
        var tournamentDecorator = new TournamentDecorator(tournamentId);

        // Assert
        Assert.Equal(tournamentId, tournamentDecorator.Tournament.TournamentId);
    }

    [Fact]
    public void Constructor_WithTournament_SetsTournament()
    {
        // Arrange
        var tournament = new Tournament { TournamentId = 123 };

        // Act
        var tournamentDecorator = new TournamentDecorator(tournament);

        // Assert
        Assert.Equal(tournament, tournamentDecorator.Tournament);
    }

    [Fact]
    public void TogglePlayerEnabled_PlayerNotMonitored_AddsMonitoredPlayer()
    {
        // Arrange
        var tournamentDecorator = new TournamentDecorator(123);
        int playerId = 1;

        // Act
        var monitoredPlayer = tournamentDecorator.TogglePlayerEnabled(playerId);

        // Assert
        Assert.NotNull(monitoredPlayer);
        Assert.Single(tournamentDecorator.MonitoredPlayers);
        Assert.Equal(playerId, tournamentDecorator.MonitoredPlayers.First().PlayerId);
    }

    [Fact]
    public void TogglePlayerEnabled_PlayerMonitored_RemovesMonitoredPlayer()
    {
        // Arrange
        var tournamentDecorator = new TournamentDecorator(123);
        int playerId = 1;
        tournamentDecorator.TogglePlayerEnabled(playerId); // Add player

        // Act
        var monitoredPlayer = tournamentDecorator.TogglePlayerEnabled(playerId); // Remove player

        // Assert
        Assert.Null(monitoredPlayer);
        Assert.Empty(tournamentDecorator.MonitoredPlayers);
    }

    [Fact]
    public void GetPlayersWithMonitoring_ReturnsPlayersWithMonitoring()
    {
        // Arrange
        var tournamentDecorator = new TournamentDecorator(123);
        var players = new Players
        {
            new Player { PlayerId = 1, Name = "Player 1" },
            new Player { PlayerId = 2, Name = "Player 2" }
        };
        tournamentDecorator.Players = players;
        tournamentDecorator.TogglePlayerEnabled(1); // Monitor Player 1

        // Act
        var result = tournamentDecorator.GetPlayersWithMonitoring();

        // Assert
        Assert.Equal(2, result.Count());
        Assert.True(result.First(p => p.PlayerId == 1).IsMonitored);
        Assert.False(result.First(p => p.PlayerId == 2).IsMonitored);
    }

    [Fact]
    public void IsFinished_TournamentFinished_ReturnsTrue()
    {
        // Arrange
        var tournament = new Tournament { Status = "Finished" };
        var tournamentDecorator = new TournamentDecorator(tournament);

        // Act
        var result = tournamentDecorator.IsFinished();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsFinished_TournamentNotFinished_ReturnsFalse()
    {
        // Arrange
        var tournament = new Tournament { Status = "Ongoing" };
        var tournamentDecorator = new TournamentDecorator(tournament);

        // Act
        var result = tournamentDecorator.IsFinished();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void PlayerMatches_ReturnsMatchesForPlayer()
    {
        // Arrange
        var tournament = new Tournament
        {
            Matches =
            [
                new() { MatchId = 1, PlayerA = new Player { PlayerId = 1 }, PlayerB = new Player { PlayerId = 2 } },
                new() { MatchId = 2, PlayerA = new Player { PlayerId = 3 }, PlayerB = new Player { PlayerId = 1 } }
            ]
        };
        var tournamentDecorator = new TournamentDecorator(tournament);

        // Act
        var result = tournamentDecorator.PlayerMatches(1);

        // Assert
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void ActivePlayerMatches_ReturnsActiveMatchesForPlayer()
    {
        // Arrange
        var tournament = new Tournament
        {
            Matches =
            [
                new() { MatchId = 1, PlayerA = new Player { PlayerId = 1 }, PlayerB = new Player { PlayerId = 2 }, MatchStatusCode = MatchStatusCode.Active },
                new() { MatchId = 2, PlayerA = new Player { PlayerId = 3 }, PlayerB = new Player { PlayerId = 1 }, MatchStatusCode = MatchStatusCode.Finished }
            ]
        };
        var tournamentDecorator = new TournamentDecorator(tournament);

        // Act
        var result = tournamentDecorator.ActivePlayerMatches(1);

        // Assert
        Assert.Single(result);
        Assert.Equal(1, result.First().MatchId);
    }

    [Fact]
    public void ResultsPlayerMatches_ReturnsFinishedMatchesForPlayer()
    {
        // Arrange
        List<Match> matches =
            [
                new() { MatchId = 1, PlayerA = new Player { PlayerId = 1 }, PlayerB = new Player { PlayerId = 2 }, MatchStatusCode = MatchStatusCode.Finished },
                new() { MatchId = 2, PlayerA = new Player { PlayerId = 3 }, PlayerB = new Player { PlayerId = 1 }, MatchStatusCode = MatchStatusCode.Active }
            ];
        var tournament = new Tournament
        {
            Matches = matches
        };
        var tournamentDecorator = new TournamentDecorator(tournament);

        // Act
        var result = tournamentDecorator.ResultsPlayerMatches(1);

        // Assert
        Assert.Single(result);
        Assert.Equal(1, result.First().MatchId);
    }
}