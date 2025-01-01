using Moq;
using PoolScoreBuddy.Domain.Models;
using PoolScoreBuddy.Domain.Models.API;
using PoolScoreBuddy.Domain.Services;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace PoolScoreBuddy.Domain.Tests.Services;

public class CueScoreAPIClientTests
{
    private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
    private readonly Mock<IResilientClientWrapper> _mockResilientClientWrapper;
    private readonly CueScoreAPIClient _cueScoreAPIClient;

    public CueScoreAPIClientTests()
    {
        _mockHttpClientFactory = new Mock<IHttpClientFactory>();
        _mockResilientClientWrapper = new Mock<IResilientClientWrapper>();
        _cueScoreAPIClient = new CueScoreAPIClient(_mockHttpClientFactory.Object, _mockResilientClientWrapper.Object);
    }

    [Fact]
    public async Task GetTournament_ValidResponse_ReturnsTournament()
    {
        // Arrange
        var dto = new TournamentDto
        {
            TournamentId = 123,
            BaseAddresses = new List<string> { "https://api.cuescore.com" },
            FallbackAddress = "https://api.cuescore.com"
        };

        var httpClient = new HttpClient();
        _mockHttpClientFactory.Setup(factory => factory.CreateClient(It.IsAny<string>())).Returns(httpClient);

        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(new Tournament { TournamentId = 123 }))
        };

        _mockResilientClientWrapper.Setup(wrapper => wrapper.FetchResponse(It.IsAny<HttpClient>(), It.IsAny<IEnumerable<string>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync(responseMessage);

        // Act
        var tournament = await _cueScoreAPIClient.GetTournament(dto);

        // Assert
        Assert.NotNull(tournament);
        Assert.Equal(123, tournament.TournamentId);
    }

    [Fact]
    public async Task GetTournament_InvalidResponse_ThrowsException()
    {
        // Arrange
        var dto = new TournamentDto
        {
            TournamentId = 123,
            BaseAddresses = new List<string> { "https://api.cuescore.com" },
            FallbackAddress = "https://api.cuescore.com"
        };

        var httpClient = new HttpClient();
        _mockHttpClientFactory.Setup(factory => factory.CreateClient(It.IsAny<string>())).Returns(httpClient);

        var responseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest);

        _mockResilientClientWrapper.Setup(wrapper => wrapper.FetchResponse(It.IsAny<HttpClient>(), It.IsAny<IEnumerable<string>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync(responseMessage);

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() => _cueScoreAPIClient.GetTournament(dto));
    }

    [Fact]
    public async Task GetPlayers_ValidResponse_ReturnsPlayers()
    {
        // Arrange
        var dto = new PlayersDto
        {
            TournamentId = 123,
            BaseAddresses = new List<string> { "https://api.cuescore.com" },
            FallbackAddress = "https://api.cuescore.com"
        };

        var httpClient = new HttpClient();
        _mockHttpClientFactory.Setup(factory => factory.CreateClient(It.IsAny<string>())).Returns(httpClient);

        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(new Players { new Player { PlayerId = 1, Name = "Player 1" } }))
        };

        _mockResilientClientWrapper.Setup(wrapper => wrapper.FetchResponse(It.IsAny<HttpClient>(), It.IsAny<IEnumerable<string>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync(responseMessage);

        // Act
        var players = await _cueScoreAPIClient.GetPlayers(dto);

        // Assert
        Assert.NotNull(players);
        Assert.Single(players);
        Assert.Equal(1, players[0].PlayerId);
    }

    [Fact]
    public async Task GetPlayers_InvalidResponse_ThrowsException()
    {
        // Arrange
        var dto = new PlayersDto
        {
            TournamentId = 123,
            BaseAddresses = new List<string> { "https://api.cuescore.com" },
            FallbackAddress = "https://api.cuescore.com"
        };

        var httpClient = new HttpClient();
        _mockHttpClientFactory.Setup(factory => factory.CreateClient(It.IsAny<string>())).Returns(httpClient);

        var responseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest);

        _mockResilientClientWrapper.Setup(wrapper => wrapper.FetchResponse(It.IsAny<HttpClient>(), It.IsAny<IEnumerable<string>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync(responseMessage);

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() => _cueScoreAPIClient.GetPlayers(dto));
    }
}