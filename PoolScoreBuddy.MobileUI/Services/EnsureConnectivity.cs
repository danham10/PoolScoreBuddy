using PoolScoreBuddy.Resources;

namespace PoolScoreBuddy.Services
{
    public class EnsureConnectivity(IAlert alert) : IEnsureConnectivity
    {
        public bool IsConnected()
        {
            NetworkAccess accessType = Connectivity.Current.NetworkAccess;
            return accessType == NetworkAccess.Internet;
        }

        public async Task<bool> IsConnectedWithAlert()
        {
            if (IsConnected())
            {
                return true;
            }
            else
            {
                await alert.Show(AppResources.AppTitle, AppResources.NoConnectivityMessage, AppResources.OK);
                return false;
            }
        }
    }
}
