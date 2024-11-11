namespace CuescoreBuddy.Models;

public record ParticipantNotification(int MatchId, string Player1, string Player2, DateTime StartTime, string TableName);