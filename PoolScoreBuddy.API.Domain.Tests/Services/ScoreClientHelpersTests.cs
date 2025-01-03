using Moq;
using PoolScoreBuddy.API.Domain.Services;
using PoolScoreBuddy.Domain.Models.API;
using System.Text.Json;

namespace PoolScoreBuddy.API.Domain.Tests.Services;

public class ScoreClientHelpersTests
{
    private readonly Mock<IScoreClient> _mockScoreClient;

    public ScoreClientHelpersTests()
    {
        _mockScoreClient = new Mock<IScoreClient>();
    }

    [Fact]
    public async Task GetPlayers_ReturnsSerializedPlayers()
    {
        // Arrange
        int tournamentId = 123;
        var players = new List<Player> { new() { PlayerId = 1, Name = "Player 1" } };
        _mockScoreClient.Setup(client => client.GetPlayers(tournamentId)).ReturnsAsync(players);

        // Act
        var result = await ScoreClientHelpers.GetPlayers(tournamentId, _mockScoreClient.Object);

        // Assert
        var expectedJson = JsonSerializer.Serialize(players);
        Assert.Equal(expectedJson, result);
    }

    [Fact]
    public void GetService_WithParticipants_ReturnsPlayers()
    {
        // Arrange
        string participants = "Participants list";

        // Act
        var result = ScoreClientHelpers.GetService(participants);

        // Assert
        Assert.Equal(ScoreEndpointTypeEnum.Players, result);
    }

    [Fact]
    public void GetService_WithoutParticipants_ReturnsTournament()
    {
        // Arrange
        string? participants = null;

        // Act
        var result = ScoreClientHelpers.GetService(participants);

        // Assert
        Assert.Equal(ScoreEndpointTypeEnum.Tournament, result);
    }

    [Fact]
    public async Task GetTournament_WithPlayerIds_ReturnsSerializedTournament()
    {
        // Arrange
        int tournamentId = 123;
        string playerIds = "1,2,3";
        int[] calledMatchIds = [1, 2];
        var tournament = new Tournament { TournamentId = tournamentId };
        _mockScoreClient.Setup(client => client.GetTournament(tournamentId, It.IsAny<int[]>(), calledMatchIds)).ReturnsAsync(tournament);

        // Act
        var result = await ScoreClientHelpers.GetTournament(tournamentId, playerIds, calledMatchIds, _mockScoreClient.Object);

        // Assert
        var expectedJson = JsonSerializer.Serialize(tournament);
        Assert.Equal(expectedJson, result);
    }

    [Fact]
    public async Task GetTournament_WithoutPlayerIds_ReturnsSerializedTournament()
    {
        // Arrange
        int tournamentId = 123;
        string? playerIds = null;
        int[] calledMatchIds = [1, 2];
        var tournament = new Tournament { TournamentId = tournamentId };
        _mockScoreClient.Setup(client => client.GetTournament(tournamentId, It.IsAny<int[]>(), calledMatchIds)).ReturnsAsync(tournament);

        // Act
        var result = await ScoreClientHelpers.GetTournament(tournamentId, playerIds, calledMatchIds, _mockScoreClient.Object);

        // Assert
        var expectedJson = JsonSerializer.Serialize(tournament);
        Assert.Equal(expectedJson, result);
    }
}