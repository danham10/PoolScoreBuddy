using PoolScoreBuddy.Domain.Services;
using RichardSzalay.MockHttp;
using System.Net;

namespace PoolScoreBuddy.Domain.Tests.Services
{
    public class ResilientClientWrapperTests
    {
        private readonly ResilientClientWrapper _resilientClientWrapper;
        private readonly MockHttpMessageHandler _mockHttpClient;

        public ResilientClientWrapperTests()
        {
            _resilientClientWrapper = new ResilientClientWrapper();
            _mockHttpClient = new MockHttpMessageHandler();
        }

        [Fact]
        public async Task FetchResponse_ValidProxyEndpoint_ReturnsSuccessResponse()
        {
            // Arrange
            var candidateEndpoints = new List<string> { "https://proxy1.com", "https://proxy2.com" };
            var fallbackEndpoint = "https://fallback.com";
            var relativeUrl = "/api/test";
            var apiAffinityId = 1;

            var successResponse = new HttpResponseMessage(HttpStatusCode.OK);
            _mockHttpClient.When("*").Respond(req => successResponse);
            var client = new HttpClient(_mockHttpClient);

            // Act
            var response = await _resilientClientWrapper.FetchResponse(client, candidateEndpoints, fallbackEndpoint, relativeUrl, apiAffinityId);

            // Assert
            Assert.Equal(successResponse, response);
        }

        [Fact]
        public async Task FetchResponse_InvalidProxyEndpoints_FallbackToRootApi()
        {
            // Arrange
            var candidateEndpoints = new List<string> { "https://proxy1.com", "https://proxy2.com" };
            var fallbackEndpoint = "https://fallback.com";
            var relativeUrl = "/api/test";
            var apiAffinityId = 1;

            var failureResponse = new HttpResponseMessage(HttpStatusCode.BadRequest);
            var successResponse = new HttpResponseMessage(HttpStatusCode.OK);

            _mockHttpClient.When("*")
                .Respond(req => failureResponse)
                .Respond(req => failureResponse)
                .Respond(req => successResponse);
            var client = new HttpClient(_mockHttpClient);

            // Act
            var response = await _resilientClientWrapper.FetchResponse(client, candidateEndpoints, fallbackEndpoint, relativeUrl, apiAffinityId);

            // Assert
            Assert.Equal(successResponse, response);
        }

        [Fact]
        public async Task FetchResponse_ProxyEndpointsAndFallback500_Returns500()
        {
            // Arrange
            var candidateEndpoints = new List<string> { "https://proxy1.com", "https://proxy2.com" };
            var fallbackEndpoint = "https://fallback.com";
            var relativeUrl = "/api/test";
            var apiAffinityId = 1;

            var failureResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError);

            _mockHttpClient.When("*")
                .Respond(req => failureResponse)
                .Respond(req => failureResponse)
                .Respond(req => failureResponse);

            var client = new HttpClient(_mockHttpClient);

            // Act
            var response = await _resilientClientWrapper.FetchResponse(client, candidateEndpoints, fallbackEndpoint, relativeUrl, apiAffinityId);

            // Assert
            Assert.Equal(failureResponse, response);
        }

        [Fact]
        public async Task PerformRequest_CuescoreBaseUrlWithPlayerIds_RemovesPlayerIdsFromUri()
        {
            // Arrange
            var baseUrl = "https://api.cuescore.com";
            var uri = "/api/test?playerIds=1,2,3";
            var expectedUri = "/api/test";

            var successResponse = new HttpResponseMessage(HttpStatusCode.OK);
            _mockHttpClient.When($"{baseUrl}{expectedUri}")
                .Respond(req => successResponse);

            var client = new HttpClient(_mockHttpClient);

            // Act
            var response = await _resilientClientWrapper.FetchResponse(client, new List<string> { baseUrl }, baseUrl, uri, 1);

            // Assert
            Assert.Equal(successResponse, response);
        }
    }
}