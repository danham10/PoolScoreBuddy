using PoolScoreBuddy.Domain.Services;

namespace PoolScoreBuddy.Domain.Tests.Services;

public class APIBalancerTests
{
    [Fact]
    public void SelectEndpoint_ValidEndpoints_ReturnsEndpoint()
    {
        // Arrange
        var candidateEndpoints = new List<string> { "https://proxy1.com", "https://proxy2.com", "https://proxy3.com" };
        var knownBadEndpoints = new List<string>();
        var apiAffinityId = 1;

        // Act
        var selectedEndpoint = APIBalancer.SelectEndpoint(candidateEndpoints, knownBadEndpoints, apiAffinityId);

        // Assert
        Assert.Equal("https://proxy2.com", selectedEndpoint);
    }

    [Fact]
    public void SelectEndpoint_KnownBadEndpoints_ExcludesBadEndpoints()
    {
        // Arrange
        var candidateEndpoints = new List<string> { "https://proxy1.com", "https://proxy2.com" };
        var knownBadEndpoints = new List<string> { "https://proxy1.com" };
        var apiAffinityId = 1;

        // Act
        var selectedEndpoint = APIBalancer.SelectEndpoint(candidateEndpoints, knownBadEndpoints, apiAffinityId);

        // Assert
        Assert.Equal("https://proxy2.com", selectedEndpoint);
    }

    [Fact]
    public void SelectEndpoint_AllEndpointsBad_ReturnsNull()
    {
        // Arrange
        var candidateEndpoints = new List<string> { "https://proxy1.com", "https://proxy2.com", "https://proxy3.com" };
        var knownBadEndpoints = new List<string> { "https://proxy1.com", "https://proxy2.com", "https://proxy3.com" };
        var apiAffinityId = 1;

        // Act
        var selectedEndpoint = APIBalancer.SelectEndpoint(candidateEndpoints, knownBadEndpoints, apiAffinityId);

        // Assert
        Assert.Null(selectedEndpoint);
    }

    [Fact]
    public void SelectEndpoint_EmptyCandidateEndpoints_ReturnsNull()
    {
        // Arrange
        var candidateEndpoints = new List<string>();
        var knownBadEndpoints = new List<string>();
        var apiAffinityId = 1;

        // Act
        var selectedEndpoint = APIBalancer.SelectEndpoint(candidateEndpoints, knownBadEndpoints, apiAffinityId);

        // Assert
        Assert.Null(selectedEndpoint);
    }

    [Fact]
    public void SelectEndpoint_ApiAffinityIdWrapsAround_ReturnsCorrectEndpoint()
    {
        // Arrange
        var candidateEndpoints = new List<string> { "https://proxy1.com", "https://proxy2.com" };
        var knownBadEndpoints = new List<string>();
        var apiAffinityId = 3; // This should wrap around to the first endpoint

        // Act
        var selectedEndpoint = APIBalancer.SelectEndpoint(candidateEndpoints, knownBadEndpoints, apiAffinityId);

        // Assert
        Assert.Equal("https://proxy2.com", selectedEndpoint);
    }
}