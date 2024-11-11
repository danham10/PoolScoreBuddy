using System.Diagnostics;
using System.Text.Json;

namespace CuescoreBuddy.Services
{
    internal class CueScoreService : ICueScoreService
    {
        private readonly HttpClient _httpClient;
        private readonly string _remoteServiceBaseUrl = "https://api.cuescore.com"; //TODO make config?

        public CueScoreService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<Tournament> GetTournament(int tournamentId)
        {
            var uri = $"{_remoteServiceBaseUrl}/tournament/?id={tournamentId}";
            var response = await _httpClient.GetAsync(uri);
            string data;

            if (response.IsSuccessStatusCode)
            {
                data = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<Tournament>(data);
            }

            throw new Exception("Cannot fetch tournament TODO improve");
        }

        public async Task<List<Player>> GetPlayers(int tournamentId)
        {
            var uri = $"{_remoteServiceBaseUrl}/tournament/?id={tournamentId}&participants=Participants+list";
            var response = await _httpClient.GetAsync(uri);
            string data;

            if (response.IsSuccessStatusCode)
            {
                data = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<Player>>(data);
            }
            throw new Exception("Cannot fetch players TODO improve");
        }
    }

    public interface ICueScoreService
    {
        public Task<Tournament> GetTournament(int tournamentId);
        public Task<List<Player>> GetPlayers(int tournamentId);
    }
}
