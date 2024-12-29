namespace PoolScoreBuddy;
public class Settings
{
    public string CueScoreBaseUrl { get; set; } = null!;
    public int APIPingIntervalSeconds { get; set; }
    public List<string> APIProxies { get; set; } = [];

    //This isnt very sensitive, our API is simply a proxy wrapper for a publicly available API anyway.
    public string JWTToken { get; set; } = null!;
}
