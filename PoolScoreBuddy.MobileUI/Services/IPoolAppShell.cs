namespace PoolScoreBuddy.Services;

public interface IPoolAppShell
{
    public Task GoToAsync(string viewName, Dictionary<string, object> navigationParameters);
}