﻿
using Polly;

namespace PoolScoreBuddy.Domain;

internal class ResilientClientWrapper(HttpClient client, IEnumerable<string> endpoints)
{
    private readonly IEnumerator<string> _endpointsIterator = endpoints.Where(p => !_knownBadEndpoints.Any(p2 => p2 == p)).GetEnumerator();
    private static IList<string> _knownBadEndpoints = [];

    internal async Task<ResilientResponse> FetchResponse(string relativeUrl)
    {
        var retryPolicyForNotSuccessAnd4xx = Policy
            .HandleResult<HttpResponseMessage?>(response => response != null && !response.IsSuccessStatusCode)
            .OrResult(response => response != null && (int)response.StatusCode > 400 && (int)response.StatusCode < 500)
            .WaitAndRetryForeverAsync(_ => TimeSpan.FromSeconds(1));

        var response = await retryPolicyForNotSuccessAnd4xx.ExecuteAsync(async () => { 
            return await GetNewAddressAndPerformRequest(relativeUrl); 
        });

        var returnedResponse = response ?? new HttpResponseMessage() { StatusCode = System.Net.HttpStatusCode.InternalServerError };

        return new ResilientResponse() { Response = returnedResponse, KnownBadEndpoints = _knownBadEndpoints };
    }

    private async Task<HttpResponseMessage?> GetNewAddressAndPerformRequest(string relativeUrl)
    {
        return _endpointsIterator.MoveNext() ? await PerformRequest(_endpointsIterator.Current, relativeUrl) : null;
    }

    private async Task<HttpResponseMessage?> PerformRequest(string baseUrl, string uri)
    {
        const string playerIdsQueryKey = "&playerIds";
        var baseUrlIsCuescore = baseUrl.Contains("api.cuescore.com", StringComparison.CurrentCultureIgnoreCase);
        var uriContainsPlayerQuery = uri.Contains(playerIdsQueryKey, StringComparison.CurrentCultureIgnoreCase);

        if (baseUrlIsCuescore && uriContainsPlayerQuery) {
            // cuescore API does not support player Ids, we cannot filter these for a smaller payload
            // Our own proxy API does.
            uri = uri.Substring(0, uri.LastIndexOf(playerIdsQueryKey));
        }

        var response = await client.GetAsync($"{baseUrl}{uri}");

        if (!response?.IsSuccessStatusCode == true)
        {
            _knownBadEndpoints.Add(baseUrl);
        }

        return response;
    }
}

