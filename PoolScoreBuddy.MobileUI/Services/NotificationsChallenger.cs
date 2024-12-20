using Plugin.LocalNotification;
using PoolScoreBuddy.Resources;

namespace PoolScoreBuddy.Services
{
    public class NotificationsChallenger : INotificationsChallenger
    {

        public async Task<bool> AllowNotificationsAsync()
        {
            var allowed = await LocalNotificationCenter.Current.AreNotificationsEnabled();

            if (!allowed)
                allowed = await LocalNotificationCenter.Current.RequestNotificationPermission();

            if (!allowed)
            {
                await Application.Current!.MainPage!.DisplayAlert(AppResources.Alert, AppResources.ManualNotificationsWarning, "OK");
            }

            return allowed;
        }
    }

    public interface INotificationsChallenger
    {
        public Task<bool> AllowNotificationsAsync();
    }
}