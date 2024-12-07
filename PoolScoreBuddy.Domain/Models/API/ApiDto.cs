namespace PoolScoreBuddy.Domain.Models.API;

public class TournamentDto() : ApiDto
{
    private IEnumerable<int>? playerIds;

    public int TournamentId { get; set; }
    public IEnumerable<int>? PlayerIds { get => BaseUrl.Contains("cuescore.com", StringComparison.CurrentCultureIgnoreCase) ? playerIds : []; set => playerIds = value; }
}

public class PlayersDto : ApiDto
{
    private IEnumerable<int> playerIds = [];

    public int TournamentId { get; set; }
}

public class ApiDto
{
    public string BaseUrl { get; set; } = "";
}
