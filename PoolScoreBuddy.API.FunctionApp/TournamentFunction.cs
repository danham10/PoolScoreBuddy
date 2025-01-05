using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using PoolScoreBuddy.API.Domain.Services;

namespace PoolScoreBuddy.API.FunctionApp;

public class TournamentFunction(ILogger<TournamentFunction> logger, IScoreClient cacheClient)
{
    private readonly ILogger<TournamentFunction> _logger = logger;
    private readonly IScoreClient _cacheClient = cacheClient;

    [Function(nameof(TournamentFunction))]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "v1/tournament")] HttpRequest req)
    {
        int tournamentId = 0;
        string? participants = null;
        string? playerIds = null;
        int[]? calledMatchIds = [];

        try
        {
            var query = req.Query;

            tournamentId = Convert.ToInt32(query["id"]);
            participants = query["participants"];
            playerIds = query["playerIds"];

            string returnVal = ScoreClientHelpers.GetService(participants) switch
            {
                ScoreEndpointTypeEnum.Tournament => await ScoreClientHelpers.GetTournament(tournamentId, playerIds, calledMatchIds, _cacheClient),
                ScoreEndpointTypeEnum.Players => await ScoreClientHelpers.GetPlayers(tournamentId, _cacheClient),
                _ => throw new NotImplementedException(),
            };

            _logger.LogInformation($"TournamentFunction# HTTP trigger processed OK", [tournamentId, participants, playerIds]);

            return new OkObjectResult(returnVal);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error processing tournament {ex}", [tournamentId, participants, playerIds]);
            return new StatusCodeResult(500);
        }
    }
}
