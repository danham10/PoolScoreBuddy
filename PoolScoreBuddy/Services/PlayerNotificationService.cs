using PoolScoreBuddy.Models.API;
using PoolScoreBuddy.Resources;
using Plugin.LocalNotification;
using Plugin.LocalNotification.AndroidOption;

namespace PoolScoreBuddy.Services;

public class PlayerNotificationService(IDataStore dataStore, IScoreAPIClient cueScoreService, INotificationService notificationService) : IPlayerNotificationService
{
    public async Task<List<CuescoreNotification>> ProcessNotifications()
    {
        List<CuescoreNotification> notifications = [];

        try
        {
            // load tournaments from API
            foreach (var tournament in dataStore.Tournaments)
            {
                await tournament.Fetch(cueScoreService, tournament.Tournament.TournamentId!.Value);
            }

            // remove tournaments that have finished
            dataStore.Tournaments.RemoveAll(t => t.IsFinished());

            foreach (var t in dataStore.Tournaments)
            {
                foreach (var p in t.MonitoredPlayers)
                {
                    AddMatchNotifications(notifications, t, p);
                    AddResultsNotifications(notifications, t, p);
                }
            }
            return notifications;
        }
        catch (Exception)
        {
            AddErrorNotification(notifications);
        }

        return notifications;
    }

    public async Task SendNotifications(List<CuescoreNotification> notifications)
    {
        foreach (var notification in notifications)
        {
            string title = $"{notification.Player1} vs {notification.Player2}";
            string text = notification.NotificationType switch
            {
                NotificationType.Start => string.Format(AppResources.StartNotification, notification.Message, notification.StartTime.ToString("h:mm tt")),
                NotificationType.Result => string.Format(AppResources.ResultNotification, notification.Message),
                _ => throw new InvalidDataException()
            };

            NotificationRequest request = new()
            {
                NotificationId = notification.MatchId,
                Title = title,
                Description = text,
                CategoryType = NotificationCategoryType.Status,
                Android =
                {
                    IconSmallName =
                    {
                        ResourceName = "cuescore_notify_icon",
                    },
                    Color =
                    {
                        ResourceName = "colorPrimary"
                    },
                    Priority = AndroidPriority.High,
                    AutoCancel = true,
                }
            };

            _ = await notificationService.Show(request);
        }
    }

    private static void AddErrorNotification(List<CuescoreNotification> notifications)
    {
        notifications.Add(new CuescoreNotification(
            NotificationType.Error,
            0,
            "",
            "",
            DateTime.Now,
            AppResources.ErrorNotification));
    }

    private static void AddResultsNotifications(List<CuescoreNotification> notifications, TournamentDecorator t, MonitoredPlayer p)
    {
        var notifiedResultsMatchIds = p.ResultsMatchIds;

        var resultsPlayerMatchIds = from m in t.ResultsPlayerMatches(p.PlayerId)
                                    select m.MatchId;

        var unnotifiedResultsMatchIds = resultsPlayerMatchIds.Except(notifiedResultsMatchIds);

        var unnotifiedResults = from tm in t.Tournament.Matches
                                where unnotifiedResultsMatchIds.Contains(tm.MatchId)
                                select tm;


        foreach (var match in unnotifiedResults)
        {
            notifications.Add(new CuescoreNotification(
                NotificationType.Result,
                match.MatchId,
                match.PlayerA.Name,
                match.PlayerB.Name,
                GetMatchTime(match.StopTime),
                $"{match.ScoreA} - {match.ScoreB}"));

            p.ResultsMatchIds.Add(match.MatchId);
        }
    }

    private static DateTime GetMatchTime(object time)
    {
        return string.IsNullOrEmpty(time.ToString()) ?
            DateTime.Now :
            DateTime.Parse(time.ToString()!, null, System.Globalization.DateTimeStyles.RoundtripKind);
    }

    private static void AddMatchNotifications(List<CuescoreNotification> notifications, TournamentDecorator t, MonitoredPlayer p)
    {
        var notifiedCalledMatchIds = p.CalledMatchIds;

        var activePlayerMatchIds = from m in t.ActivePlayerMatches(p.PlayerId)
                                   select m.MatchId;

        var unnotifiedActiveMatchIds = activePlayerMatchIds.Except(notifiedCalledMatchIds);

        var unnotifiedMatches = from tm in t.Tournament.Matches
                                where unnotifiedActiveMatchIds.Contains(tm.MatchId)
                                select tm;

        foreach (var match in unnotifiedMatches)
        {
            notifications.Add(new CuescoreNotification(
                NotificationType.Start,
                match.MatchId,
                match.PlayerA.Name,
                match.PlayerB.Name,
                GetMatchTime(match.StartTime),
                match.GetTable().Name!));

            p.CalledMatchIds.Add(match.MatchId);
        }
    }
}
