using PoolScoreBuddy.Resources;

namespace PoolScoreBuddy
{
    internal static class EnsureConnectivity
    {
        public static bool IsConnected()
        {
            NetworkAccess accessType = Connectivity.Current.NetworkAccess;
            return accessType == NetworkAccess.Internet;
        }

        public static async Task<bool> IsConnectedWithAlert()
        {
            if (IsConnected())
            {
                return true;
            } else
            {
                await Application.Current!.MainPage!.DisplayAlert(AppResources.AppTitle, AppResources.NoConnectivityMessage, AppResources.OK);
                return false;
            }
        }
    }
}
