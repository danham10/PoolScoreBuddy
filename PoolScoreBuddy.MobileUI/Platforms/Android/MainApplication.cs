using Android.App;
using Android.OS;
using Android.Runtime;
using PoolScoreBuddy.Domain;

namespace PoolScoreBuddy;

[Application]
public class MainApplication(IntPtr handle, JniHandleOwnership ownership) : MauiApplication(handle, ownership)
{
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "Notification channel block is only invoked on Oreo or later")]
    public override void OnCreate()
    {
        base.OnCreate();

        if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
        {
            var serviceChannel =
               new NotificationChannel(Constants.ChannelId, Constants.ChannelName, NotificationImportance.Default);

            if (GetSystemService(NotificationService) is NotificationManager manager)
            {
                manager.CreateNotificationChannel(serviceChannel);
            }
        }
    }
}
