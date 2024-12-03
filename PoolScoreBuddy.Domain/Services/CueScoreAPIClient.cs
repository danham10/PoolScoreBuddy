using System.Text.Json;
using PoolScoreBuddy.Domain.Models;
using PoolScoreBuddy.Domain.Models.API;

namespace PoolScoreBuddy.Domain.Services;

public class CueScoreAPIClient : IScoreAPIClient
{
    private HttpClient _httpClient;
    private readonly static JsonSerializerOptions _serializerOptions = new() { PropertyNameCaseInsensitive = true };

    public CueScoreAPIClient(HttpClient httpClient)
    {
#if DEBUG
        HttpClientHandler insecureHandler = GetInsecureHandler();
        _httpClient = new HttpClient(insecureHandler);
#else
    _httpClient = httpClient;
#endif
    }

    public async Task<Tournament> GetTournament(string baseUrl, int tournamentId, int? playerId = null)
    {
#if DEBUG
        HttpClientHandler insecureHandler = GetInsecureHandler();
        _httpClient = new HttpClient(insecureHandler);
#else
    HttpClient client = new HttpClient();
#endif

        var uri = $"{baseUrl}/tournament?id={tournamentId}";
        var response = await _httpClient.GetAsync(uri);

        try
        {
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {

            throw;
        }
   

        string data = await response.Content.ReadAsStringAsync();

        return Deserialize<Tournament>(data);
    }

    public async Task<List<Player>> GetPlayers(string baseUrl, int tournamentId)
    {
        var uri = $"{baseUrl}/tournament/?id={tournamentId}&participants=Participants+list";
        var response = await _httpClient.GetAsync(uri);

        response.EnsureSuccessStatusCode();

        string data = await response.Content.ReadAsStringAsync();
        return Deserialize<List<Player>>(data);
    }

    private static T Deserialize<T>(string json)
    {
        const string expectedErrorPrefix = "{error: ";

        // Below might be returned - invalid JSON unfortunately. So we need to return it verbatim.
        // error: 'Could not find tournament with given ID.'
        if (json.StartsWith(expectedErrorPrefix, StringComparison.CurrentCultureIgnoreCase))
        {
            throw new APIServerException(json);
        }

        return JsonSerializer.Deserialize<T>(json, _serializerOptions)!;
    }

    //https://learn.microsoft.com/en-us/previous-versions/xamarin/cross-platform/deploy-test/connect-to-local-web-services#bypass-the-certificate-security-check
    private HttpClientHandler GetInsecureHandler()
    {
        HttpClientHandler handler = new HttpClientHandler();
        handler.ServerCertificateCustomValidationCallback =
            (message, cert, chain, errors) =>
            {
                if (cert.Issuer.Equals("CN=localhost"))
                    return true;
                return errors == System.Net.Security.SslPolicyErrors.None;
            };
        return handler;
    }
}
