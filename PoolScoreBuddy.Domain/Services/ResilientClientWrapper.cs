﻿
using Polly;

namespace PoolScoreBuddy.Domain.Services;

public class ResilientClientWrapper() : IResilientClientWrapper
{
    private readonly static IList<string> _knownBadEndpoints = [];

    public async Task<HttpResponseMessage?> FetchResponse(HttpClient client, IEnumerable<string> candidateEndpoints, string fallbackEndpoint, string relativeUrl, int apiAffinityId)
    {
        var retryPolicyForNotSuccessAnd401 = Policy
            .HandleResult<HttpResponseMessage?>(response => response != null && !response.IsSuccessStatusCode)
            .OrResult(response => response != null && ((int)response.StatusCode > 400 && (int)response.StatusCode < 500))
            .WaitAndRetryForeverAsync(_ => TimeSpan.FromSeconds(1));

        // Attempt a proxy API
        await retryPolicyForNotSuccessAnd401.ExecuteAsync(async () =>
        {
            string? proxyEndpoint = APIBalancer.SelectEndpoint(candidateEndpoints, _knownBadEndpoints, apiAffinityId);

            return (proxyEndpoint != null) ? await PerformRequest(client, proxyEndpoint, relativeUrl) : null;
        });

        // Fallback to root API (cuescore)
        return await PerformRequest(client, fallbackEndpoint, relativeUrl);
    }

    private async Task<HttpResponseMessage?> PerformRequest(HttpClient client, string baseUrl, string uri)
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

