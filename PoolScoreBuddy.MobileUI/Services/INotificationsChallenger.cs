namespace PoolScoreBuddy.Services;

public interface INotificationsChallenger
{
    public Task<bool> AllowNotificationsAsync();
}