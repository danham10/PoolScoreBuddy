using Microsoft.Extensions.Caching.Memory;
using Moq;
using PoolScoreBuddy.Domain.Models.API;
using PoolScoreBuddy.Domain.Services;
using PoolScoreBuddy.API.Domain.Services;
using Match = PoolScoreBuddy.Domain.Models.API.Match;

namespace PoolScoreBuddy.API.Domain.Tests.Services
{
    public class ScoreClientTests
    {
        private readonly Mock<IScoreAPIClient> _mockScoreAPIClient;
        private readonly Mock<IMemoryCache> _mockCache;
        private readonly Mock<ISettings> _mockSettings;
        private readonly ScoreClient _scoreClient;

        public ScoreClientTests()
        {
            _mockScoreAPIClient = new Mock<IScoreAPIClient>();
            _mockCache = new Mock<IMemoryCache>();
            _mockSettings = new Mock<ISettings>();
            _mockSettings.Setup(o => o.GetSetting<string>("CueScoreBaseUrl")).Returns("https://api.cuescore.com");
            _mockSettings.Setup(o => o.GetSetting<int>("APIPingIntervalSeconds")).Returns(60);

            _scoreClient = new ScoreClient(_mockScoreAPIClient.Object, _mockCache.Object, _mockSettings.Object);
        }

        [Fact]
        public async Task GetTournament_CacheMiss_FetchesFromAPI()
        {
            // Arrange
            int tournamentId = 123;
            var tournament = new Tournament { TournamentId = tournamentId };
            var cacheKey = $"t:{tournamentId}";

            object dummy;
            _mockCache
                .Setup(c => c.TryGetValue(cacheKey, out dummy!))
                .Returns(false);

            _mockCache
                .Setup(c => c.CreateEntry(It.IsAny<object>()))
                .Returns(Mock.Of<ICacheEntry>);

            _mockScoreAPIClient.Setup(api => api.GetTournament(It.IsAny<ApiDto>())).ReturnsAsync(tournament);

            // Act
            var result = await _scoreClient.GetTournament(tournamentId, null, null);

            // Assert
            Assert.Equal(tournamentId, result.TournamentId);
            _mockScoreAPIClient.Verify(api => api.GetTournament(It.IsAny<ApiDto>()), Times.Once);
            _mockCache.Verify(c => c.CreateEntry(cacheKey), Times.Once);
        }

        [Fact]
        public async Task GetTournament_CacheHit_ReturnsFromCache()
        {
            // Arrange
            int tournamentId = 123;
            var tournament = new Tournament { TournamentId = tournamentId };
            var cacheKey = $"t:{tournamentId}";

            object dummy = tournament;
            _mockCache
                .Setup(c => c.TryGetValue(cacheKey, out dummy!))
                .Returns(true);

            // Act
            var result = await _scoreClient.GetTournament(tournamentId, null, null);

            // Assert
            Assert.Equal(tournamentId, result.TournamentId);
            _mockScoreAPIClient.Verify(api => api.GetTournament(It.IsAny<ApiDto>()), Times.Never);
        }

        [Fact]
        public async Task GetPlayers_CacheMiss_FetchesFromAPI()
        {
            // Arrange
            int tournamentId = 123;
            var players = new Players { new Player { PlayerId = 1, Name = "Player 1" } };
            var cacheKey = $"p:{tournamentId}";

            object dummy;
            _mockCache
                .Setup(c => c.TryGetValue(cacheKey, out dummy!))
                .Returns(false);

            _mockCache
                .Setup(c => c.CreateEntry(It.IsAny<object>()))
                .Returns(Mock.Of<ICacheEntry>);

            _mockScoreAPIClient.Setup(api => api.GetPlayers(It.IsAny<ApiDto>())).ReturnsAsync(players);

            // Act
            var result = await _scoreClient.GetPlayers(tournamentId);

            // Assert
            Assert.Single(result);
            Assert.Equal(1, result.First().PlayerId);
            _mockScoreAPIClient.Verify(api => api.GetPlayers(It.IsAny<ApiDto>()), Times.Once);
            _mockCache.Verify(c => c.CreateEntry(cacheKey), Times.Once);
        }

        [Fact]
        public async Task GetPlayers_CacheHit_ReturnsFromCache()
        {
            // Arrange
            int tournamentId = 123;
            var players = new Players { new Player { PlayerId = 1, Name = "Player 1" } };
            var cacheKey = $"p:{tournamentId}";

            object dummy = players;
            _mockCache
                .Setup(c => c.TryGetValue(cacheKey, out dummy!))
                .Returns(true);

            // Act
            var result = await _scoreClient.GetPlayers(tournamentId);

            // Assert
            Assert.Single(result);
            Assert.Equal(1, result.First().PlayerId);
            _mockScoreAPIClient.Verify(api => api.GetPlayers(It.IsAny<ApiDto>()), Times.Never);
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