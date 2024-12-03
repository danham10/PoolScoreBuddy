using PoolScoreBuddy.API.Services;

namespace PoolScoreBuddy.API.Endpoints;

public static class TournamentEndpoints
{
    public static void RegisterTournamentEndpoints(this IEndpointRouteBuilder routes)
    {
        var users = routes.MapGroup("/api/v1/tournament");

        users.MapGet("/", async (int id, int? playerId, int[]? calledMatchIds, IScoreClient cacheClient) => await cacheClient.GetTournament(id, playerId, calledMatchIds));
    }
}
