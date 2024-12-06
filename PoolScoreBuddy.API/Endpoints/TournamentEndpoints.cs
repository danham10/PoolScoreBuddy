﻿using Microsoft.Extensions.Configuration;
using PoolScoreBuddy.API.Services;
using System.Text.Json;

namespace PoolScoreBuddy.API.Endpoints;

public static class TournamentEndpoints
{
    public static void RegisterTournamentEndpoints(this IEndpointRouteBuilder routes)
    {

        var users = routes.MapGroup("/api/v1/tournament");

        users.MapGet("/", async (int id, string? participants, string? playerIds, int[]? calledMatchIds, IScoreClient cacheClient, IConfiguration configuration) =>
        {
            return ScoreClientHelpers.GetService(participants) switch
            {
                ScoreEndpointTypeEnum.Tournament => await ScoreClientHelpers.GetTournament(id, participants, playerIds, calledMatchIds, cacheClient),
                ScoreEndpointTypeEnum.Players => await ScoreClientHelpers.GetPlayers(id, cacheClient),
                _ => throw new NotImplementedException(),
            };
        });
    }
}
