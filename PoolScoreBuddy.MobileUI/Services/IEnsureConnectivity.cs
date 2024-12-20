namespace PoolScoreBuddy.Services;

public interface IEnsureConnectivity
{
    public bool IsConnected();

    public Task<bool> IsConnectedWithAlert();
}