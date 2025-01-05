using System.Text.Json;
using PoolScoreBuddy.Domain.Models;
using PoolScoreBuddy.Domain.Models.API;

namespace PoolScoreBuddy.Domain.Services;

public class CueScoreAPIClient(IHttpClientFactory httpClientFactory, IResilientClientWrapper resilientClientWrapper) : IScoreAPIClient
{
    private readonly static JsonSerializerOptions _serializerOptions = new() { PropertyNameCaseInsensitive = true };

    public async Task<Tournament> GetTournament(TournamentDto dto)
    {
        var httpClient = httpClientFactory.CreateClient(Constants.HttpClientName);

        string? playerQueryValue = GetPlayerQueryValue(dto.PlayerIds);

        var uri = $"tournament?id={dto.TournamentId}{playerQueryValue}";

        var response = await resilientClientWrapper.FetchResponse(httpClient, dto.BaseAddresses, dto.FallbackAddress, uri, dto.TournamentId);

        response!.EnsureSuccessStatusCode();

        string data = await response.Content.ReadAsStringAsync();
        return Deserialize<Tournament>(data);
    }


    public async Task<Players> GetPlayers(PlayersDto dto)
    {
        var httpClient = httpClientFactory.CreateClient(Constants.HttpClientName);

        var uri = $"tournament/?id={dto.TournamentId}&participants=Participants+list";
        var response = await resilientClientWrapper.FetchResponse(httpClient, dto.BaseAddresses, dto.FallbackAddress, uri, dto.TournamentId)  ;

        response!.EnsureSuccessStatusCode();

        string data = await response.Content.ReadAsStringAsync();
        return Deserialize<Players>(data);
    }

    private static T Deserialize<T>(string json) where T : IErrorContainer, new()
    {
        const string expectedErrorPrefix = "{error: ";

        // Below might be returned - invalid JSON unfortunately. So we need to return it verbatim.
        // error: 'Could not find tournament with given ID.'
        if (json.StartsWith(expectedErrorPrefix, StringComparison.CurrentCultureIgnoreCase))
        {
            return new T() { Error = json };
        }

        return JsonSerializer.Deserialize<T>(json, _serializerOptions)!;
    }

    private static string? GetPlayerQueryValue(IEnumerable<int>? playerIds)
    {
        if (playerIds == null) return null;

        var q = string.Join<string>(",", playerIds.Select(p => p.ToString()));

        return playerIds.Any() ? $"&playerIds={q}" : null;
    }
}
