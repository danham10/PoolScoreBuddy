﻿using PoolScoreBuddy.Domain.Services;
using PoolScoreBuddy.Domain.Models.API;
using PoolScoreBuddy.Resources;
using Plugin.LocalNotification;
using Plugin.LocalNotification.AndroidOption;
using Microsoft.Extensions.Logging;

namespace PoolScoreBuddy.Services;

public class PlayerNotificationService(ITournamentService tournamentService, 
    IScoreAPIClient cueScoreService, 
    INotificationService notificationService, 
    ISettingsResolver settingsResolver,
    ILogger<PlayerNotificationService> logger) : IPlayerNotificationService
{
    public async Task<List<PlayerNotification>> ProcessNotifications()
    {
        List<PlayerNotification> notifications = [];
        var settings = settingsResolver.GetSettings();

        try
        {
            // load tournaments from API
            foreach (var tournament in tournamentService.Tournaments)
            {
                IEnumerable<int> monitoredPlayerIds = from player in tournament.MonitoredPlayers
                                           select player.PlayerId;

                ApiDto dto = new()
                {
                    FunctionKey = settings.FunctionKey,
                    BaseAddresses = settings.APIProxies,
                    FallbackAddress = settings.CueScoreBaseUrl,
                    TournamentId = tournament.Tournament.TournamentId,
                    PlayerIds = monitoredPlayerIds
                };


                var updatedTournament = await cueScoreService.GetTournament(dto);
                tournament.Tournament = updatedTournament;
            }

            // remove tournaments that have finished
            tournamentService.RemoveFinished();

            foreach (var t in tournamentService.Tournaments)
            {
                foreach (var p in t.MonitoredPlayers.ToList())
                {
                    AddMatchNotifications(notifications, t, p);
                    AddResultsNotifications(notifications, t, p);
                }
            } 
            return notifications;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing notifications");
            AddErrorNotification(notifications);
        }

        return notifications;
    }

    public async Task SendNotifications(List<PlayerNotification> notifications)
    {
        foreach (var notification in notifications)
        {
            string title = notification.NotificationType switch
            {
                NotificationType.Start => $"{notification.Player1} vs {notification.Player2}",
                NotificationType.Result => $"{notification.Player1} vs {notification.Player2}",
                NotificationType.Error => AppResources.Alert,
                _ => throw new InvalidDataException()
            };


            string text = notification.NotificationType switch
            {
                NotificationType.Start => string.Format(AppResources.StartNotification, notification.Message, notification.StartTime.ToString("h:mm tt")),
                NotificationType.Result => string.Format(AppResources.ResultNotification, notification.Message),
                NotificationType.Error => notification.Message,
                _ => throw new InvalidDataException()
            };

            NotificationRequest request = new()
            {
                NotificationId = notification.MatchId,
                Title = title,
                Description = text,
                CategoryType = NotificationCategoryType.Status,
                Silent = false,
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

    private static void AddErrorNotification(List<PlayerNotification> notifications)
    {
        notifications.Add(new PlayerNotification(
            NotificationType.Error,
            0,
            "",
            "",
            DateTime.Now,
            AppResources.ErrorNotification));
    }

    private static void AddResultsNotifications(List<PlayerNotification> notifications, ITournamentDecorator t, MonitoredPlayer p)
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
            notifications.Add(new PlayerNotification(
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

    private static void AddMatchNotifications(List<PlayerNotification> notifications, ITournamentDecorator t, MonitoredPlayer p)
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
            notifications.Add(new PlayerNotification(
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
