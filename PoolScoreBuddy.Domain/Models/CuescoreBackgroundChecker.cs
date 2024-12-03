namespace PoolScoreBuddy.Domain.Models;

public record CuescoreBackgroundChecker(ServiceMessageType MessageType);

public enum ServiceMessageType
{
    Default = 0,
}
