using PoolScoreBuddy.API.Services;
using System.Text.Json;

internal static class ScoreClientHelpers
{
    internal static async Task<string> GetPlayers(int id, IScoreClient cacheClient) => JsonSerializer.Serialize(await cacheClient.GetPlayers(id));

    /// <summary>
    /// //Need to mirror CueScores own API (https://api.cuescore.com/), so one endpoint for Tournaments and Players (!)
    /// </summary>
    /// <param name="participants"></param>
    /// <returns></returns>
    internal static ScoreEndpointTypeEnum GetService(string? participants)
    {
        return string.IsNullOrEmpty(participants) switch
        {
            true => ScoreEndpointTypeEnum.Tournament,
            false => ScoreEndpointTypeEnum.Players,
        };
    }

    internal static async Task<string> GetTournament(int id, string? playerIds, int[]? calledMatchIds, IScoreClient cacheClient)
    {
        int[] playerIdArray = [];

        if (!string.IsNullOrEmpty(playerIds))
        {
            playerIdArray = playerIds!.Split(",").Select(x => Convert.ToInt32(x)).ToArray();
        }

        return JsonSerializer.Serialize(await cacheClient.GetTournament(id, playerIdArray, calledMatchIds));
    }
}