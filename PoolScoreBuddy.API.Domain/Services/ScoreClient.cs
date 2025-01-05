using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using PoolScoreBuddy.Domain.Models.API;
using PoolScoreBuddy.Domain.Services;

namespace PoolScoreBuddy.API.Domain.Services;

public class ScoreClient(IScoreAPIClient scoreAPIClient, IMemoryCache cache, ISettings settings) : IScoreClient
{
    const string tournamentCacheFormatter = "t:{0}";
    const string playersCacheFormatter = "p:{0}";

    public async Task<Tournament> GetTournament(int tournamentId, int[]? playerIds, int[]? notifiedMatchIds)
    {
        string cacheKey = string.Format(tournamentCacheFormatter, tournamentId);

        if (!cache.TryGetValue(cacheKey, out Tournament? tournament))
        {
            TournamentDto dto = new()
            {
                FallbackAddress = settings.GetSetting<string>("CueScoreBaseUrl"),
                TournamentId = tournamentId,
                PlayerIds = playerIds
            };

            tournament = await scoreAPIClient.GetTournament(dto);

            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromSeconds(settings.GetSetting<int>("APIPingIntervalSeconds")));

            cache.Set(cacheKey, tournament, cacheEntryOptions);
        }

        if (playerIds?.Length > 0)
        {
            tournament!.Matches?.RemoveAll(m => ShouldRemoveMatch(m, playerIds, notifiedMatchIds));
        }

        return tournament!;
    }
    public async Task<IEnumerable<Player>> GetPlayers(int tournamentId)
    {
        string cacheKey = string.Format(playersCacheFormatter, tournamentId);

        if (!cache.TryGetValue(cacheKey, out Players? players))
        {
            PlayersDto dto = new()
            {
                FallbackAddress = settings.GetSetting<string>("CueScoreBaseUrl"),
                TournamentId = tournamentId,
            };

            players = await scoreAPIClient.GetPlayers(dto);

            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromSeconds(settings.GetSetting<int>("APIPingIntervalSeconds")));

            cache.Set(cacheKey, players, cacheEntryOptions);
        }

        return players!;
    }


    internal static bool ShouldRemoveMatch(Match m, int[]? playerIds, int[]? notifiedMatchIds)
    {
        bool monitoredMatches = playerIds != null && (playerIds.Contains(m.PlayerA.PlayerId) || playerIds.Contains(m.PlayerB.PlayerId));
        bool alreadyNotified = notifiedMatchIds != null && notifiedMatchIds.Contains(m.MatchId);

        return !monitoredMatches || alreadyNotified;
    }
}