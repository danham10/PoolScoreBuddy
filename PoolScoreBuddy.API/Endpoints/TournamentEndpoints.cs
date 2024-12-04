using PoolScoreBuddy.API.Services;

namespace PoolScoreBuddy.API.Endpoints;

public static class TournamentEndpoints
{
    public static void RegisterTournamentEndpoints(this IEndpointRouteBuilder routes)
    {
        var users = routes.MapGroup("/api/v1/tournament");

        //Need to mirror CueScores own API (https://api.cuescore.com/), so one endpoint for Tournaments and Players (!)
        users.MapGet("/", async (int id, string? participants, string? playerIds, int[]? calledMatchIds, IScoreClient cacheClient) =>
        {
            int[] playerIdArray = [];

            if (!string.IsNullOrEmpty(playerIds))
            {
                playerIdArray = playerIds!.Split(",").Select(x => Convert.ToInt32(x)).ToArray();
            }

            return await cacheClient.GetTournament(id, participants, playerIdArray, calledMatchIds);
        });
    }
}
