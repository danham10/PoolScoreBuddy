using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using PoolScoreBuddy.Di;
using PoolScoreBuddy.Domain;
using PoolScoreBuddy.Domain.Models;
using PoolScoreBuddy.Domain.Models.API;
using PoolScoreBuddy.Domain.Services;

namespace PoolScoreBuddy.API.Services;

public class ScoreClient(IScoreAPIClient scoreAPIClient, IMemoryCache cache, IOptions<Settings> options) : IScoreClient
{
    const string tournamentCacheFormatter = "t:{0}";
    const string playersCacheFormatter = "p:{0}";

    public async Task<Tournament> GetTournament(int tournamentId, string? participants, int[]? playerIds, int[]? notifiedMatchIds)
    {
        string cacheKey = string.Format(tournamentCacheFormatter, tournamentId);

        if (!cache.TryGetValue(cacheKey, out Tournament? tournament))
        {
            TournamentDto dto = new()
            {
                ApiProviderType = ApiProviderType.CueScore,
                TournamentId = tournamentId,
                PlayerIds = playerIds
            };

            tournament = await scoreAPIClient.GetTournament(dto);

            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromSeconds(options.Value.APIPingIntervalSeconds));

            cache.Set(cacheKey, tournament, cacheEntryOptions);
        }

        if (playerIds?.Length > 0)
        {
            var removedMatchCount = tournament!.Matches?.RemoveAll(m => ShouldRemoveMatch(m, playerIds, notifiedMatchIds));
            Console.WriteLine($"removed {removedMatchCount} matches");
        }

        return tournament!;
    }
    public async Task<IEnumerable<Player>> GetPlayers(int tournamentId)
    {
        string cacheKey = string.Format(playersCacheFormatter, tournamentId);

        if (!cache.TryGetValue(cacheKey, out List<Player>? players))
        {
            PlayersDto dto = new()
            {
                ApiProviderType = ApiProviderType.CueScore,
                TournamentId = tournamentId,
            };

            players = await scoreAPIClient.GetPlayers(dto);

            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromSeconds(options.Value.APIPingIntervalSeconds));

            cache.Set(cacheKey, players, cacheEntryOptions);
        }

        return players!;
    }


    private static bool ShouldRemoveMatch(Match m, int[]? playerIds, int[]? notifiedMatchIds)
    {
        bool monitoredMatches = playerIds != null && (playerIds.Contains(m.PlayerA.PlayerId) || playerIds.Contains(m.PlayerB.PlayerId));
        bool alreadyNotified = notifiedMatchIds != null && notifiedMatchIds.Contains(m.MatchId);

        return !monitoredMatches || alreadyNotified;
    }
}