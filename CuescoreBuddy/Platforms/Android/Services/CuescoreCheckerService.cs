using Android.App;
using Android.Content;
using Android.OS;
using Android.Util;
using AndroidX.Core.App;

namespace CuescoreBuddy.Platforms;

[Service(ForegroundServiceType = Android.Content.PM.ForegroundService.TypeDataSync)]
public class AndroidCuescoreCheckerService : Service
{
    static readonly string? Tag = typeof(AndroidCuescoreCheckerService).FullName;
    CancellationTokenSource _cts = new();
    IDataStore dataStore = ServiceResolver.GetService<IDataStore>();
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
            RegisterForegroundService("");

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
        return intent!.Action != null && intent.Action.Equals(Constants.ActionStopService) || !dataStore.Tournaments.MonitoredPlayers().Any();
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

    void RegisterForegroundService(string playerName)
    {
        var stopServiceIntent = new Intent(this, GetType());
        stopServiceIntent.SetAction(Constants.ActionStopService);
        var stopServicePendingIntent = PendingIntent.GetService(this, 0, stopServiceIntent, PendingIntentFlags.Immutable);

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

        await Task.Run(async () =>
        {
            if (tokenSource.Token.IsCancellationRequested)
            {

            }

            while (true)
            {
                tokenSource.Token.ThrowIfCancellationRequested();

                try
                {
                    var organiser = new NotificationService(dataStore, cueScoreService);
                    var notificationsToSend = await organiser.ProcessNotifications();

                    foreach (var notification in notificationsToSend)
                    {
                        Notification? deviceNotification = null;

                        switch (notification.type)
                        {
                            case NotificationType.Start:
                                deviceNotification = new NotificationCompat.Builder(this, Constants.ChannelId)
                                    .SetContentTitle($"{notification.Player1} vs {notification.Player2}")
                                    .SetContentText($"Table {notification.Message} {notification.StartTime.ToString("h:mm tt")}")
                                    .SetSmallIcon(Resource.Drawable.cuescore_notify_icon)
                                    .SetAutoCancel(true)
                                    .Build();
                                break;
                            case NotificationType.Result:
                                deviceNotification = new NotificationCompat.Builder(this, Constants.ChannelId)
                                    .SetContentTitle($"{notification.Player1} vs {notification.Player2}")
                                    .SetContentText($"Result {notification.Message}")
                                    .SetSmallIcon(Resource.Drawable.cuescore_notify_icon)
                                    .SetAutoCancel(true)
                                    .Build();
                                break;
                        }


                        notificationManager.Notify(notification.MatchId, deviceNotification);
                    }

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
        var notificationIntent = new Intent(this, typeof(MainActivity));
        var pendingIntent = PendingIntent.GetActivity(this, 0, notificationIntent, PendingIntentFlags.Immutable);

        return new NotificationCompat.Builder(this, Constants.ChannelId)
           .SetContentTitle("Cuescore buddy")
           .SetContentText($"Monitoring {(_isCheckerRunning ? "started" : "stopped. No players or active tournaments to monitor.")}")
           .SetSmallIcon(Resource.Drawable.cuescore_notify_icon)
           .SetContentIntent(pendingIntent)
           .Build();
    }
}
