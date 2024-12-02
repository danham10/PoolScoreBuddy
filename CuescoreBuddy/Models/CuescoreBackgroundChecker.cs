namespace CuescoreBuddy.Models;

public record CuescoreBackgroundChecker(ServiceMessageType MessageType);

public enum ServiceMessageType
{
    Default = 0,
}
