
using Polly;
using System.Net;

namespace PoolScoreBuddy.Domain.Services;

internal class ResilientClientWrapper(HttpClient client, IEnumerable<string> candidateEndpoints, string fallbackEndpoint)
{
    private readonly static IList<string> _knownBadEndpoints = [];

    internal async Task<HttpResponseMessage?> FetchResponse(string relativeUrl, int apiAffinityId)
    {
        var retryPolicyForNotSuccessAnd401 = Policy
            .HandleResult<HttpResponseMessage?>(response => response != null && !response.IsSuccessStatusCode && (int)response.StatusCode != 401 /*&& !response.IsCuescore TODO how to do this??*/)
            //.OrResult(response => response != null && ((int)response.StatusCode > 400 && (int)response.StatusCode < 500))
            .WaitAndRetryForeverAsync(_ => TimeSpan.FromSeconds(1));

        string? proxyEndpoint = EndpointSelector.SelectEndpoint(candidateEndpoints, _knownBadEndpoints, apiAffinityId.ToString());
        
        if (proxyEndpoint == null)
        {
            return await PerformRequest(fallbackEndpoint, relativeUrl);
        }
            
        return await retryPolicyForNotSuccessAnd401.ExecuteAsync(async () =>
        {
            string endpoint = proxyEndpoint;
            return await PerformRequest(endpoint, relativeUrl);
        });
    }
    private async Task<HttpResponseMessage?> PerformRequest(string baseUrl, string uri)
    { 
        const string playerIdsQueryKey = "&playerIds";
        var baseUrlIsCuescore = baseUrl.Contains("api.cuescore.com", StringComparison.CurrentCultureIgnoreCase);
        var uriContainsPlayerQuery = uri.Contains(playerIdsQueryKey, StringComparison.CurrentCultureIgnoreCase);

        if (baseUrlIsCuescore && uriContainsPlayerQuery)
        {
            // cuescore API does not support player Ids, we cannot filter these for a smaller payload
            // Our own proxy API does.
            uri = uri[..uri.LastIndexOf(playerIdsQueryKey)];
        }

        var response = await client.GetAsync($"{baseUrl}{uri}");

        if (!response?.IsSuccessStatusCode == true)
        {
            _knownBadEndpoints.Add(baseUrl);
        }

        return response;
    }
}

