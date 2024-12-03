namespace PoolScoreBuddy.Domain;

public static class Constants
{
    public const int ServiceRunningNotificationId = 10000;
    public const string ChannelId = "PoolBuddyServiceChannel";
    public const string ChannelName = "PoolBuddyServiceChannel";

    public const string ActionStartService = "PoolBuddy.Action.StartService";
    public const string ActionStopService = "PoolBuddy.Action.StopService";
    public const string APIBaseUrl = "https://10.0.2.2:7181/api/v1"; //localhost
    public const string CueScoreBaseUrl = "https://api.cuescore.com";

    public const int APIPingIntervalSeconds = 60;
}