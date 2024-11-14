namespace CuescoreBuddy.Models
{
    public record CuescoreBackgroundChecker(ServiceMessageType type);

    public enum ServiceMessageType {
        Default = 0,
    }
}
