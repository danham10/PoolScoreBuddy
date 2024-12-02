namespace PoolScoreBuddy.Models;

public enum NotificationType
{
    Start,
    Result,
    Error
}
public record CuescoreNotification(NotificationType NotificationType, int MatchId, string Player1, string Player2, DateTime StartTime, string Message);