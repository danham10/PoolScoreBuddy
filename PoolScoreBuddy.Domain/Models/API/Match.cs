using System.Text.Json;

namespace PoolScoreBuddy.Domain.Models.API;

public enum MatchStatusCode
{
    Active = 1,
    Finished = 2
}

public class Match
{
    public int MatchId { get; set; }
    public required Player PlayerA { get; set; }
    public required Player PlayerB { get; set; }
    public int ScoreA { get; set; }
    public int ScoreB { get; set; }
    public object Table { get; set; } = null!;
    public int TournamentId { get; set; }
    public object StartTime { get; set; } = null!;
    public object StopTime { get; set; } = null!;
    public MatchStatusCode MatchStatusCode { get; set; }

    /// <summary>
    /// Table returns inconsistent data. Either empty array or a Table object.
    /// </summary>
    /// <returns></returns>
    public Table GetTable()
    {
        JsonSerializerOptions options = new() { PropertyNameCaseInsensitive = true };

        try
        {
            return JsonSerializer.Deserialize<Table>(Table.ToString() ?? "{}", options)!;
        }
        catch (Exception)
        {
            return new Table();
        }
    }
}
