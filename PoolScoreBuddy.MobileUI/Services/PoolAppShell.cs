namespace PoolScoreBuddy.Services;

public class PoolAppShell : IPoolAppShell
{
    public async Task GoToAsync(ShellNavigationState shellNavigationState, Dictionary<string, object> navigationParameters)
    {
        await Shell.Current.GoToAsync(shellNavigationState, false, navigationParameters);
    }
}
