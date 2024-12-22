namespace PoolScoreBuddy.Services;

public class PoolAppShell : IPoolAppShell
{
    public async Task GoToAsync(string viewName, Dictionary<string, object> navigationParameters)
    {
        await Shell.Current.GoToAsync(viewName, false, navigationParameters);
    }
}
