using System.Text.Json;
using PoolScoreBuddy.Models.API;

namespace PoolScoreBuddy.Services
{
    internal class CueScoreAPIClient(HttpClient httpClient) : IScoreAPIClient
    {
        const string RemoteServiceBaseUrl = "https://api.cuescore.com"; //TODO make config?

        private readonly HttpClient _httpClient = httpClient;
        private readonly static JsonSerializerOptions _serializerOptions = new() { PropertyNameCaseInsensitive = true };

        public async Task<Tournament> GetTournament(int tournamentId)
        {
            var uri = $"{RemoteServiceBaseUrl}/tournament/?id={tournamentId}";
            var response = await _httpClient.GetAsync(uri);

            response.EnsureSuccessStatusCode();

            string data = await response.Content.ReadAsStringAsync();

            return Deserialize<Tournament>(data);
        }

        public async Task<List<Player>> GetPlayers(int tournamentId)
        {
            var uri = $"{RemoteServiceBaseUrl}/tournament/?id={tournamentId}&participants=Participants+list";
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
    }
}
