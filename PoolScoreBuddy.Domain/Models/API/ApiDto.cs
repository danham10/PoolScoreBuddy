namespace PoolScoreBuddy.Domain.Models.API;

public record ApiDto
{
    public int TournamentId { get; set; }
    public string Uri { get; set; }
    public IEnumerable<int>? PlayerIds { get; set; }
    public string FunctionKey { get; set; } = null!;
    public List<string> BaseAddresses { get; set; } = [];
    public string FallbackAddress { get; set; } = null!;
}
