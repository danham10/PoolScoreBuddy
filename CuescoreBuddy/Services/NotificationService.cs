using CuescoreBuddy.Models.API;

namespace CuescoreBuddy.Services;

public class NotificationService(IDataStore dataStore, IScoreAPIClient cueScoreService)
{
    public async Task<List<CuescoreNotification>> ProcessNotifications()
    {
        List<CuescoreNotification> notifications = new();

        try
        {
            // load tournaments from API
            foreach( var tournament in dataStore.Tournaments)
            {
                await tournament.Fetch(cueScoreService, tournament.Tournament.TournamentId);
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

    private static void AddErrorNotification(List<CuescoreNotification> notifications)
    {
        notifications.Add(new CuescoreNotification(
            NotificationType.Error,
            0,
            "",
            "",
            DateTime.Now,
            $"There has been a monitoring error, suggest you monitor matches yourself."));
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
                DateTime.Parse(match.StopTime.ToString()!, null, System.Globalization.DateTimeStyles.RoundtripKind),
                $"{match.ScoreA.ToString()} - {match.ScoreB.ToString()}"));

            p.ResultsMatchIds.Add(match.MatchId);
        }
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
                DateTime.Parse(match.StartTime.ToString()!, null, System.Globalization.DateTimeStyles.RoundtripKind),
                match.GetTable().Name!));

            p.CalledMatchIds.Add(match.MatchId);
        }
    }
}
