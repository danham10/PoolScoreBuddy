namespace PoolScoreBuddy.Domain;

public static class Constants
{
    public static readonly int ServiceRunningNotificationId = 10000;
    public static readonly string ChannelId = "PoolBuddyServiceChannel";
    public static readonly string ChannelName = "PoolBuddyServiceChannel";

    public static readonly string ActionStartService = "PoolBuddy.Action.StartService";
    public static readonly string ActionStopService = "PoolBuddy.Action.StopService";


    public static readonly int APIPingIntervalSeconds = 60; 
    public static readonly string HttpClientName = "PoolBuddyHttpClient";

    //This isnt very sensitive, our API is simply a proxy wrapper for a publicly available API anyway.
    public static readonly string JWTToken = "AfewBreakD!shesW0UldBn!ce";

    /// <summary>
    /// Maui does not support AppSettings, so for shared settings this will do.
    /// </summary>
#if DEBUG
    public static readonly string APIBaseUrl = "https://10.0.2.2:7181/api/v1/"; //localhost
#else
    public static readonly string APIBaseUrl = "https://TBC/api/v1/"; //Azure
#endif

    public static readonly string CueScoreBaseUrl = "https://api.cuescore.com/";
    public static readonly string[] ClientBaseUrls = [APIBaseUrl, CueScoreBaseUrl];
}