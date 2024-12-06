namespace PoolScoreBuddy;
public class Settings
{
    public string? CueScoreBaseUrl { get; set; }
    public int APIPingIntervalSeconds { get; set; }
    public APISettings API { get; set; } = null!;
}

public class APISettings
{
    public string BaseUrl { get; set; } = null!;
    public string JWTToken { get; set; } = null!;
}
