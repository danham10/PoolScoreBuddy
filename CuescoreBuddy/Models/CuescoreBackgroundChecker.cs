namespace CuescoreBuddy.Models;

public record CuescoreBackgroundChecker(ServiceMessageType messageType);

public enum ServiceMessageType
{
    Default = 0,
}
