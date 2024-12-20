namespace PoolScoreBuddy.Services;

public interface IPoolAppShell
{
    public Task GoToAsync(ShellNavigationState shellNavigationState, Dictionary<string, object> navigationParameters);
}