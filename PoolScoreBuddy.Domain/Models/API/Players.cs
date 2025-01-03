namespace PoolScoreBuddy.Domain.Models.API;

public class Players : List<Player>, IErrorContainer
{
    public Players() : base() { }

    public Players(IEnumerable<Player> players) : base(players) { }

    public string? Error { get; set; }
}