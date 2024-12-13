using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Util;
using AndroidX.Core.App;
using PoolScoreBuddy.Domain;
using PoolScoreBuddy.Domain.Services;
using PoolScoreBuddy.Resources;

namespace PoolScoreBuddy.Platforms.Android.Services;

[Service(ForegroundServiceType = ForegroundService.TypeDataSync)]
public class AndroidCuescoreCheckerService() : Service
{
    static readonly string? Tag = typeof(AndroidCuescoreCheckerService).FullName;
    readonly CancellationTokenSource _cts = new();
    readonly IDataStore dataStore = ServiceResolver.GetService<IDataStore>();
    bool _isCheckerRunning;

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

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "Compatibility is enforced in if block")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "Compatibility is enforced in if block")]
    void RegisterForegroundService()
    {
        var notification = GetServiceNotification();

        if (Build.VERSION.SdkInt < BuildVersionCodes.Tiramisu)
        {
            StartForeground(Constants.ServiceRunningNotificationId, notification);
        }
        else
        {
            StartForeground(Constants.ServiceRunningNotificationId, notification, ForegroundService.TypeDataSync);
        }
    }

    public async Task RunCheck(CancellationTokenSource tokenSource)
    {
        var notificationManager = (NotificationManager)GetSystemService(NotificationService)!;
        var playerNotificationService = ServiceResolver.GetService<IPlayerNotificationService>();
        var settings = SettingsResolver.GetSettings();

        await Task.Run(async () =>
        {
            bool onlineStatus = true;

            while (true)
            {
                tokenSource.Token.ThrowIfCancellationRequested();

                try
                {
                    if (EnsureConnectivity.IsConnected())
                    {
                        if (!onlineStatus)
                        {
                            onlineStatus = true;
                            // We have just come back online
                            var notification = GetOnlineNotification(true);
                            notificationManager.Notify(Constants.ServiceRunningNotificationId, notification);
                        }

                        var notificationsToSend = await playerNotificationService.ProcessNotifications();
                        await playerNotificationService.SendNotifications(notificationsToSend);
                    }
                    else
                    {
                        if (onlineStatus)
                        {
                            onlineStatus = false;
                            // We have just gone offline
                            var notification = GetOnlineNotification(false);
                            notificationManager.Notify(Constants.ServiceRunningNotificationId, notification);
                        }
                    }

                    if (!dataStore.Tournaments.ShouldMonitor())
                    {
                        OnDestroy();
                    }

                    await Task.Delay(settings.APIPingIntervalSeconds * 1000, tokenSource.Token);
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

    private Notification GetOnlineNotification(bool online)
    {
        // Cannot use Plugin.Localnotification because we need Android specific type for StartForeground
        var notificationIntent = new Intent(this, typeof(MainActivity));
        var pendingIntent = PendingIntent.GetActivity(this, 0, notificationIntent, PendingIntentFlags.Immutable);

        string text = online ? AppResources.ConnectivityResumedMonitoringMessage : AppResources.NoConnectivityMonitoringMessage;

        return new NotificationCompat.Builder(this, Constants.ChannelId)
           .SetContentTitle(AppResources.AppTitle)
           .SetContentText(text)
           .SetSmallIcon(_Microsoft.Android.Resource.Designer.ResourceConstant.Drawable.cuescore_notify_icon)
           .SetContentIntent(pendingIntent)
           .Build();
    }
}
