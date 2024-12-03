using Microsoft.Extensions.Caching.Memory;
using PoolScoreBuddy.Domain;
using PoolScoreBuddy.Domain.Models.API;
using PoolScoreBuddy.Domain.Services;

namespace PoolScoreBuddy.API.Services;

public class ScoreClient(IScoreAPIClient scoreAPIClient, IMemoryCache cache) : IScoreClient
{
    public async Task<Tournament> GetTournament(int id, int? playerId, int[]? notifiedMatchIds)
    {
        if (!cache.TryGetValue(id, out Tournament? tournament))
        {
            tournament = await scoreAPIClient.GetTournament(Constants.CueScoreBaseUrl, id, playerId);

            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromSeconds(Constants.APIPingIntervalSeconds));

            cache.Set(id, tournament, cacheEntryOptions);
        }

        if (playerId.HasValue)
        {
            var removedMatchCount = tournament!.Matches?.RemoveAll(m => ShouldRemoveMatch(m, playerId, notifiedMatchIds));
            Console.WriteLine($"removed {removedMatchCount} matches");
        }

        return tournament!;
    }

    //TODO getPlayers!

    private static bool ShouldRemoveMatch(Match m, int? playerId, int[]? notifiedMatchIds)
    {
        bool differentPlayers = m.PlayerA.PlayerId != playerId && m.PlayerB.PlayerId != playerId;
        bool alreadyNotified = notifiedMatchIds != null && notifiedMatchIds.Contains(m.MatchId);

        return differentPlayers || alreadyNotified;
    }
}