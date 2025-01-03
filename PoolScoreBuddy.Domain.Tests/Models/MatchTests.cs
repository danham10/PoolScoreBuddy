using PoolScoreBuddy.Domain.Models.API;
using Match = PoolScoreBuddy.Domain.Models.API.Match;

namespace PoolScoreBuddy.Domain.Tests.Models
{
    public class MatchTests
    {
        [Fact]
        public void GetTable_ValidTableObject_ReturnsTable()
        {
            // Arrange
            string tableJson = """
            {
                "Name": "Table 1"
            }
            """;

            var match = new Match
            {
                PlayerA = new Player(),

                PlayerB = new Player(),
                Table = tableJson
            }; 

            // Act
            var table = match.GetTable();

            // Assert
            Assert.NotNull(table);
            Assert.Equal("Table 1", table.Name);
        }

        [Fact]
        public void GetTable_EmptyArray_ReturnsNewTable()
        {
            // Arrange
            var match = new Match
            {
                PlayerA = new Player(),
                PlayerB = new Player(),
                Table = Array.Empty<object>()
            };

            // Act
            var table = match.GetTable();

            // Assert
            Assert.NotNull(table);
            Assert.Null(table.Name);
        }

        [Fact]
        public void GetTable_InvalidJson_ReturnsNewTable()
        {
            // Arrange
            var match = new Match
            {
                PlayerA = new Player(),
                PlayerB = new Player(),
                Table = "invalid json"
            };

            // Act
            var table = match.GetTable();

            // Assert
            Assert.NotNull(table);
            Assert.Null(table.Name);
        }
    }
}