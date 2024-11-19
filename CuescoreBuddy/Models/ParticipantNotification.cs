namespace CuescoreBuddy.Models;

public record PlayerNotification(int MatchId, string Player1, string Player2, DateTime StartTime, string TableName);