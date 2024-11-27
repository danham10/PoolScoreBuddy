using System.Text.Json;
using CuescoreBuddy.Models.API;

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
            return JsonSerializer.Deserialize<Tournament>(data, _serializerOptions)!;
        }

        public async Task<List<Player>> GetPlayers(int tournamentId)
        {
            var uri = $"{RemoteServiceBaseUrl}/tournament/?id={tournamentId}&participants=Participants+list";
            var response = await _httpClient.GetAsync(uri);

            response.EnsureSuccessStatusCode();

            string data = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<Player>>(data, _serializerOptions)!;
        }
    }
}
