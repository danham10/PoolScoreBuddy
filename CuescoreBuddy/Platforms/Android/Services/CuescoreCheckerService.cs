using Android.App;
using Android.Content;
using Android.OS;
using Android.Util;
using AndroidX.Core.App;
using CuescoreBuddy.Services;
using Plugin.LocalNotification;

namespace CuescoreBuddy.Platforms;

[Service(ForegroundServiceType = Android.Content.PM.ForegroundService.TypeDataSync)]
public class CuescoreCheckerService : Service
{
    static readonly string TAG = typeof(CuescoreCheckerService).FullName;
    CancellationTokenSource _cts = new();
    DataStore dataStore = ServiceResolver.GetService<DataStore>();
    bool _stopping;
    bool _isCheckerRunning;

    public override IBinder OnBind(Intent intent)
    {
        return null;
    }


    public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
    {
        
        var cueScoreService = ServiceResolver.GetService<ICueScoreService>();



        
        
        if (!_isCheckerRunning)
        {
            _isCheckerRunning = true;
            Task.Run(() =>
            {
                Console.WriteLine("Task is running");
                try 
                {
                    RunCheck(_cts).Wait();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }, _cts.Token);

            
            
        }
        if (intent.Action != null && intent.Action.Equals(Constants.ACTION_STOP_SERVICE) || !dataStore.Tournaments.MonitoredPlayers().Any())
        {
            Log.Info(TAG, "OnStartCommand: The service is stopping.");
            _cts.Cancel();
        }
        else
        {
            var monitoredPlayerNames = string.Join(",", dataStore.Tournaments.MonitoredPlayers());
            monitoredPlayerNames.Remove(monitoredPlayerNames.Length - 1);
            RegisterForegroundService(monitoredPlayerNames);

        }

        return StartCommandResult.Sticky;
    }

    public override void OnDestroy()
    {
        // We need to shut things down.
        dataStore.Tournaments.CancelMonitoredPlayers();

        Log.Debug(TAG, "The TimeStamper has been disposed.");
        Log.Info(TAG, "OnDestroy: The started service is shutting down.");

        // Remove the notification from the status bar.
        var notificationManager = (NotificationManager)GetSystemService(NotificationService);
        notificationManager.Cancel(Constants.SERVICE_RUNNING_NOTIFICATION_ID);

        _isCheckerRunning = false;
        
    }

    void RegisterForegroundService(string playerName)
    {

        var stopServiceIntent = new Intent(this, GetType());
        stopServiceIntent.SetAction(Constants.ACTION_STOP_SERVICE);
        var stopServicePendingIntent = PendingIntent.GetService(this, 0, stopServiceIntent, PendingIntentFlags.Immutable);

        var notificationIntent = new Intent(this, typeof(MainActivity));
        var pendingIntent = PendingIntent.GetActivity(this, 0, notificationIntent, PendingIntentFlags.Immutable);

        var notification = new NotificationCompat.Builder(this, MainApplication.ChannelId)
           .SetContentTitle("Cuescore buddy")
           .SetContentText($"Listening started") //TODO create names
           .SetSmallIcon(Resource.Drawable.ic_stat_name)
           .SetContentIntent(pendingIntent)
           .Build();

        var fsType = new Android.Content.PM.ForegroundService();

        if (Build.VERSION.SdkInt < BuildVersionCodes.Tiramisu)
        {
            StartForeground(Constants.SERVICE_RUNNING_NOTIFICATION_ID, notification);
        }
        else
        {
            StartForeground(Constants.SERVICE_RUNNING_NOTIFICATION_ID, notification, Android.Content.PM.ForegroundService.TypeDataSync);
        }
    }


    /// <summary>
    /// Builds the Notification.Action that will allow the user to stop the service via the
    /// notification in the status bar
    /// </summary>
    /// <returns>The stop service action.</returns>
    NotificationCompat.Action BuildStopServiceAction()
    {
        var stopServiceIntent = new Intent(this, GetType());
        stopServiceIntent.SetAction(Constants.ACTION_STOP_SERVICE);
        var stopServicePendingIntent = PendingIntent.GetService(this, 0, stopServiceIntent, PendingIntentFlags.Immutable);

        var builder = new NotificationCompat.Action.Builder(Android.Resource.Drawable.IcMediaPause,
                                                      "Stop service",
                                                      stopServicePendingIntent);
        return builder.Build();

    }

    public override bool StopService(Intent? name)
    {
        return base.StopService(name);
    }

    public async Task RunCheck(CancellationTokenSource tokenSource)
    {
        var dataStore = ServiceResolver.GetService<DataStore>();
        var cueScoreService = ServiceResolver.GetService<ICueScoreService>();
        var notificationManager = (NotificationManager)GetSystemService(NotificationService);

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
                    var organiser = new TournamentOrganiserService(dataStore, cueScoreService);
                    var notificationsToSend = await organiser.ProcessNotifications();

                    foreach (var notification in notificationsToSend)
                    {
                        var matchNotification = new NotificationCompat.Builder(this, MainApplication.ChannelId)
                       .SetContentTitle($"You are on table {notification.TableName}")
                       .SetContentText($"{notification.StartTime.ToString("h:mm tt")} {notification.Player1} vs {notification.Player2} ") //TODO create names
                       .SetSmallIcon(Resource.Drawable.ic_stat_name)
                       .SetAutoCancel(true);

                        notificationManager.Notify(notification.MatchId, matchNotification.Build());
                    }

                    if (!dataStore.Tournaments.ShouldMonitor())
                    {
                        OnDestroy();
                    }

                    await Task.Delay(60000, tokenSource.Token); //TODO make config

                    System.Diagnostics.Debug.WriteLine("Checking tournament API for AmIOnYet!!");
                }
                catch (TaskCanceledException)
                {
                    _stopping = true;
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
}
