namespace PoolScoreBuddy.Domain;

public static class Constants
{
    public static readonly int ServiceRunningNotificationId = 10000;
    public static readonly int MaximumMonitoredPlayersPerTournament = 10;

    public static readonly string ChannelId = "PoolBuddyServiceChannel";
    public static readonly string ChannelName = "PoolBuddyServiceChannel";

    public static readonly string ActionStartService = "PoolBuddy.Action.StartService";
    public static readonly string ActionStopService = "PoolBuddy.Action.StopService";

    public static readonly string HttpClientName = "PoolBuddyHttpClient";
}