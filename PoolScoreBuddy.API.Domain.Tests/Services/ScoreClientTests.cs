using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Moq;
using PoolScoreBuddy.Domain.Models.API;
using PoolScoreBuddy.Domain.Services;
using PoolScoreBuddy.API.Domain.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Match = PoolScoreBuddy.Domain.Models.API.Match;

namespace PoolScoreBuddy.API.Domain.Tests.Services
{
    public class ScoreClientTests
    {
        private readonly Mock<IScoreAPIClient> _mockScoreAPIClient;
        private readonly Mock<IMemoryCache> _mockCache;
        private readonly Mock<IOptions<Settings>> _mockOptions;
        private readonly ScoreClient _scoreClient;

        public ScoreClientTests()
        {
            _mockScoreAPIClient = new Mock<IScoreAPIClient>();
            _mockCache = new Mock<IMemoryCache>();
            _mockOptions = new Mock<IOptions<Settings>>();
            _mockOptions.Setup(o => o.Value).Returns(new Settings { CueScoreBaseUrl = "https://api.cuescore.com", APIPingIntervalSeconds = 60 });

            _scoreClient = new ScoreClient(_mockScoreAPIClient.Object, _mockCache.Object, _mockOptions.Object);
        }

        [Fact]
        public async Task GetTournament_CacheMiss_FetchesFromAPI()
        {
            // Arrange
            int tournamentId = 123;
            var tournament = new Tournament { TournamentId = tournamentId };
            var cacheKey = $"t:{tournamentId}";

            _mockCache.Setup(c => c.TryGetValue(cacheKey, out It.Ref<Tournament?>.IsAny)).Returns(false);
            _mockScoreAPIClient.Setup(api => api.GetTournament(It.IsAny<TournamentDto>())).ReturnsAsync(tournament);

            // Act
            var result = await _scoreClient.GetTournament(tournamentId, null, null);

            // Assert
            Assert.Equal(tournamentId, result.TournamentId);
            _mockScoreAPIClient.Verify(api => api.GetTournament(It.IsAny<TournamentDto>()), Times.Once);
            _mockCache.Verify(c => c.Set(cacheKey, tournament, It.IsAny<MemoryCacheEntryOptions>()), Times.Once);
        }

        [Fact]
        public async Task GetTournament_CacheHit_ReturnsFromCache()
        {
            // Arrange
            int tournamentId = 123;
            var tournament = new Tournament { TournamentId = tournamentId };
            var cacheKey = $"t:{tournamentId}";

            _mockCache.Setup(c => c.TryGetValue(cacheKey, out tournament)).Returns(true);

            // Act
            var result = await _scoreClient.GetTournament(tournamentId, null, null);

            // Assert
            Assert.Equal(tournamentId, result.TournamentId);
            _mockScoreAPIClient.Verify(api => api.GetTournament(It.IsAny<TournamentDto>()), Times.Never);
        }

        [Fact]
        public async Task GetPlayers_CacheMiss_FetchesFromAPI()
        {
            // Arrange
            int tournamentId = 123;
            var players = new Players { new Player { PlayerId = 1, Name = "Player 1" } };
            var cacheKey = $"p:{tournamentId}";

            _mockCache.Setup(c => c.TryGetValue(cacheKey, out It.Ref<Players?>.IsAny)).Returns(false);
            _mockScoreAPIClient.Setup(api => api.GetPlayers(It.IsAny<PlayersDto>())).ReturnsAsync(players);

            // Act
            var result = await _scoreClient.GetPlayers(tournamentId);

            // Assert
            Assert.Single(result);
            Assert.Equal(1, result.First().PlayerId);
            _mockScoreAPIClient.Verify(api => api.GetPlayers(It.IsAny<PlayersDto>()), Times.Once);
            _mockCache.Verify(c => c.Set(cacheKey, players, It.IsAny<MemoryCacheEntryOptions>()), Times.Once);
        }

        [Fact]
        public async Task GetPlayers_CacheHit_ReturnsFromCache()
        {
            // Arrange
            int tournamentId = 123;
            var players = new Players { new Player { PlayerId = 1, Name = "Player 1" } };
            var cacheKey = $"p:{tournamentId}";

            _mockCache.Setup(c => c.TryGetValue(cacheKey, out players)).Returns(true);

            // Act
            var result = await _scoreClient.GetPlayers(tournamentId);

            // Assert
            Assert.Single(result);
            Assert.Equal(1, result.First().PlayerId);
            _mockScoreAPIClient.Verify(api => api.GetPlayers(It.IsAny<PlayersDto>()), Times.Never);
        }

        [Fact]
        public void ShouldRemoveMatch_MonitoredMatchNotNotified_ReturnsFalse()
        {
            // Arrange
            var match = new Match
            {
                MatchId = 1,
                PlayerA = new Player { PlayerId = 1 },
                PlayerB = new Player { PlayerId = 2 }
            };
            int[] playerIds = { 1 };
            int[] notifiedMatchIds = { };

            // Act
            var result = ScoreClient.ShouldRemoveMatch(match, playerIds, notifiedMatchIds);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void ShouldRemoveMatch_NotMonitoredMatch_ReturnsTrue()
        {
            // Arrange
            var match = new Match
            {
                MatchId = 1,
                PlayerA = new Player { PlayerId = 3 },
                PlayerB = new Player { PlayerId = 4 }
            };
            int[] playerIds = { 1, 2 };
            int[] notifiedMatchIds = { };

            // Act
            var result = ScoreClient.ShouldRemoveMatch(match, playerIds, notifiedMatchIds);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void ShouldRemoveMatch_AlreadyNotifiedMatch_ReturnsTrue()
        {
            // Arrange
            var match = new Match
            {
                MatchId = 1,
                PlayerA = new Player { PlayerId = 1 },
                PlayerB = new Player { PlayerId = 2 }
            };
            int[] playerIds = { 1 };
            int[] notifiedMatchIds = { 1 };

            // Act
            var result = ScoreClient.ShouldRemoveMatch(match, playerIds, notifiedMatchIds);

            // Assert
            Assert.True(result);
        }
    }
}