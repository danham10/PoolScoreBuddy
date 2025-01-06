namespace PoolScoreBuddy.Domain.Services;

public interface IResilientClientWrapper
{
    Task<HttpResponseMessage?> FetchResponse(HttpClient client, IEnumerable<string> candidateEndpoints, string fallbackEndpoint, string relativeUrl, int apiAffinityId, string functionKey);
}