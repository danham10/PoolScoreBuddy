using Microsoft.Extensions.Caching.Memory;
using PoolScoreBuddy.Domain;
using PoolScoreBuddy.Domain.Models.API;
using PoolScoreBuddy.Domain.Services;
using System.Text.Json;

namespace PoolScoreBuddy.API.Services;

public class ScoreClient(IScoreAPIClient scoreAPIClient, IMemoryCache cache) : IScoreClient
{
    public async Task<string> GetTournament(int tournamentId, string? participants, int[]? playerIds, int[]? notifiedMatchIds)
    {
        if (string.IsNullOrEmpty(participants))
        {
            const string tournamentCacheFormatter = "t:{0}";
            string cacheKey = string.Format(tournamentCacheFormatter, tournamentId);

            if (!cache.TryGetValue(cacheKey, out Tournament? tournament))
            {
                tournament = await scoreAPIClient.GetTournament(Constants.CueScoreBaseUrl, tournamentId, playerIds);

                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromSeconds(Constants.APIPingIntervalSeconds));

                cache.Set(cacheKey, tournament, cacheEntryOptions);
            }

            if (playerIds != null)
            {
                var removedMatchCount = tournament!.Matches?.RemoveAll(m => ShouldRemoveMatch(m, playerIds, notifiedMatchIds));
                Console.WriteLine($"removed {removedMatchCount} matches");
            }

            return JsonSerializer.Serialize(tournament)!;
        }
        else
        {
            const string playersCacheFormatter = "p:{0}";
            string cacheKey = string.Format(playersCacheFormatter, tournamentId);

            if (!cache.TryGetValue(cacheKey, out List<Player>? players))
            {
                players = await scoreAPIClient.GetPlayers(Constants.CueScoreBaseUrl, tournamentId);

                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromSeconds(Constants.APIPingIntervalSeconds));

                cache.Set(cacheKey, players, cacheEntryOptions);
            }

            return JsonSerializer.Serialize(players)!;
        }

    }

    private static bool ShouldRemoveMatch(Match m, int[]? playerIds, int[]? notifiedMatchIds)
    {
        bool monitoredMatches = playerIds != null && (playerIds.Contains(m.PlayerA.PlayerId) || playerIds.Contains(m.PlayerB.PlayerId));
        bool alreadyNotified = notifiedMatchIds != null && notifiedMatchIds.Contains(m.MatchId);

        return !monitoredMatches || alreadyNotified;
    }
}