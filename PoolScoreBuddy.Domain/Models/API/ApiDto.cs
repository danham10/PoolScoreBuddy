namespace PoolScoreBuddy.Domain.Models.API;

public record TournamentDto() : ApiDto
{
    const string CuescoreUrl = "cuescore.com";

    private IEnumerable<int>? playerIds;

    public int TournamentId { get; set; }
    public IEnumerable<int>? PlayerIds { get => ApiProviderType != ApiProviderType.CueScore ? playerIds : []; set => playerIds = value; }
}

public record PlayersDto : ApiDto
{
    public int TournamentId { get; set; }
}

public record ApiDto
{
    //public string BaseUrl { get; set; } = "";
    public ApiProviderType ApiProviderType { get; set; } = ApiProviderType.CueScoreProxy;
}
