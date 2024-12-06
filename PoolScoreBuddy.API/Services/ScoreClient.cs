using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using PoolScoreBuddy.Di;
using PoolScoreBuddy.Domain;
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
            tournament = await scoreAPIClient.GetTournament(options.Value.CueScoreBaseUrl, tournamentId, playerIds);

            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromSeconds(options.Value.APIPingIntervalSeconds));

            cache.Set(cacheKey, tournament, cacheEntryOptions);
        }

        if (playerIds != null)
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
            players = await scoreAPIClient.GetPlayers(options.Value.CueScoreBaseUrl, tournamentId);

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