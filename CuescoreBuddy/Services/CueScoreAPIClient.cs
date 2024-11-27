using System.Text.Json;
using CuescoreBuddy.Models.API;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CuescoreBuddy.Services
{
    internal class CueScoreAPIClient : IScoreAPIClient
    {
        const string RemoteServiceBaseUrl = "https://api.cuescore.com"; //TODO make config?

        private readonly HttpClient _httpClient;
        private static JsonSerializerOptions _serializerOptions = new() { PropertyNameCaseInsensitive = true };

        public CueScoreAPIClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

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
            try
            {
                return JsonSerializer.Deserialize<T>(json, _serializerOptions)!;
            }
            catch (JsonException)
            {
                // Below might be returned - invalid JSON unfortunately. So we need to return it verbatim.
                // error: 'Could not find tournament with given ID.'
                throw new APIException(json);
            }
        }
    }
}
