﻿using System.Text.Json;

namespace CuescoreBuddy.Models;

public class Match
{
    public int MatchId { get; set; }
    public required Player PlayerA { get; set; }
    public required Player PlayerB { get; set; }
    public int ScoreA { get; set; }
    public int ScoreB { get; set; }
    public required object Table { get; set; }
    public int TournamentId { get; set; }
    public required object StartTime { get; set; }
    public required object StopTime { get; set; }
    public int MatchStatusCode { get; set; }

    /// <summary>
    /// Table returns inconsistent data. Either empty array or a Table object.
    /// </summary>
    /// <returns></returns>
    public Table GetTable()
    {
        try
        {
            return JsonSerializer.Deserialize<Table>(Table.ToString() ?? "{}")!;
        }
        catch (Exception)
        {
            return new Table();
        }
    }
}
