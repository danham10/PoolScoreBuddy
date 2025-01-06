namespace PoolScoreBuddy.Domain.Models.API;

/// <summary>
/// Generated from special paste of JSON from https://api.cuescore.com/
/// Unused properties are removed to reduce memory footprint and chance of deserialisation error
/// </summary>
public partial class Tournament : IErrorContainer
{
    public int TournamentId { get; set; }
    public string? Name { get; set; }
    public string? Status { get; set; }
    public List<Match>? Matches { get; set; } = [];

    public string? Error { get; set; }

    public bool Loaded { get; set; }
}