using Android.App;
using Android.Content;
using Android.OS;
using Android.Util;
using AndroidX.Core.App;
using CuescoreBuddy.Resources;

namespace CuescoreBuddy.Platforms;

[Service(ForegroundServiceType = Android.Content.PM.ForegroundService.TypeDataSync)]
public class AndroidCuescoreCheckerService : Service
{
    static readonly string? Tag = typeof(AndroidCuescoreCheckerService).FullName;
    readonly CancellationTokenSource _cts = new();
    readonly IDataStore dataStore = ServiceResolver.GetService<IDataStore>();
    bool _isCheckerRunning;

    const int CheckIntervalMS = 60000;

    public override IBinder OnBind(Intent? intent)
    {
        return null!;
    }

    public override StartCommandResult OnStartCommand(Intent? intent, StartCommandFlags flags, int startId)
    {
        var cueScoreService = ServiceResolver.GetService<IScoreAPIClient>();

        if (!_isCheckerRunning)
        {
            _isCheckerRunning = true;
            RegisterForegroundService();

            Task.Run(() =>
            {
                Log.Info(Tag, "Task is running");
                try
                {
                    RunCheck(_cts).Wait();
                }
                catch (Exception e)
                {
                    Log.Info(Tag, e.ToString());
                    throw;
                }
            }, _cts.Token);
        }

        if (ShouldStop(intent))
        {
            Log.Info(Tag, "OnStartCommand: The service is stopping.");
            _cts.Cancel();
        }

        return StartCommandResult.Sticky;
    }

    private bool ShouldStop(Intent? intent)
    {
        return intent!.Action != null && intent.Action.Equals(Constants.ActionStopService) || dataStore.Tournaments.MonitoredPlayers().Count == 0;
    }

    public override void OnDestroy()
    {
        // We need to shut things down.
        dataStore.Tournaments.CancelMonitoredPlayers();

        _isCheckerRunning = false;

        var notificationManager = (NotificationManager)GetSystemService(NotificationService)!;
        var notification = GetServiceNotification();
        notificationManager.Notify(Constants.ServiceRunningNotificationId, notification);

        base.OnDestroy();
    }

    void RegisterForegroundService()
    {
        var notification = GetServiceNotification();

        if (Build.VERSION.SdkInt < BuildVersionCodes.Tiramisu)
        {
            StartForeground(Constants.ServiceRunningNotificationId, notification);
        }
        else
        {
#pragma warning disable CA1416 // Validate platform compatibility
            // Compatibility is enforced in if block above
            StartForeground(Constants.ServiceRunningNotificationId, notification, Android.Content.PM.ForegroundService.TypeDataSync);
#pragma warning restore CA1416 // Validate platform compatibility
        }
    }

    public async Task RunCheck(CancellationTokenSource tokenSource)
    {
        IDataStore dataStore = ServiceResolver.GetService<IDataStore>();
        var cueScoreService = ServiceResolver.GetService<IScoreAPIClient>();
        var notificationManager = (NotificationManager)GetSystemService(NotificationService)!;
        var playerNotificationService = ServiceResolver.GetService<IPlayerNotificationService>(); //TODO remove

        await Task.Run(async () =>
        {
            while (true)
            {
                tokenSource.Token.ThrowIfCancellationRequested();

                try
                {
                    //var organiser = new PlayerNotificationService(dataStore, cueScoreService, notificationService);
                    var notificationsToSend = await playerNotificationService.ProcessNotifications();
                    await playerNotificationService.SendNotifications(notificationsToSend);

                    if (!dataStore.Tournaments.ShouldMonitor())
                    {
                        OnDestroy();
                    }

                    await Task.Delay(CheckIntervalMS, tokenSource.Token);
                }
                catch (TaskCanceledException)
                {
                    StopForeground(StopForegroundFlags.Remove);
                    StopSelf();
                    _isCheckerRunning = false;
                    break;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex);
                }
            }
        }, tokenSource.Token);
    }

    private Notification GetServiceNotification()
    {
        // Cannot use Plugin.Localnotification because we need Android specific type for StartForeground
        var notificationIntent = new Intent(this, typeof(MainActivity));
        var pendingIntent = PendingIntent.GetActivity(this, 0, notificationIntent, PendingIntentFlags.Immutable);

        return new NotificationCompat.Builder(this, Constants.ChannelId)
           .SetContentTitle(AppResources.AppTitle)
           .SetContentText(_isCheckerRunning ? AppResources.MonitoringStarted : AppResources.MonitoringStopped)
           .SetSmallIcon(_Microsoft.Android.Resource.Designer.ResourceConstant.Drawable.cuescore_notify_icon)
           .SetContentIntent(pendingIntent)
           .Build();
    }
}
