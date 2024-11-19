namespace CuescoreBuddy.Services
{

    public class TournamentOrganiserService(DataStore dataStore, ICueScoreService cueScoreService)
    {
        public async Task<List<PlayerNotification>> ProcessNotifications()
        {
            List<PlayerNotification> matchesToNotify = new();


            try
            {
                // load tournaments from API
                foreach( var tournament in dataStore.Tournaments)
                {
                    await tournament.Fetch(cueScoreService, tournament.Tournament.tournamentId);
                }


                // remove tournaments that have finished
                dataStore.Tournaments.RemoveAll(t => t.IsFinished());

                List<PlayerNotification> notifications = new();

                // for each tournament
                foreach (var t in dataStore.Tournaments)
                {
                    foreach (var p in t.MonitoredPlayers)
                    {
                        var notifiedPlayerMatchIds = p.CalledMatchIds;

                        var activePlayerMatchIds = t.ActivePlayerMatches(p.PlayerId).Select(m => m.matchId).ToList();
                        //var activePlayerMatchIds = from m  in t.Tournament.ActivePlayerMatches(p.PlayerId)
                        //                           select m.matchId;

                        var unnotifiedActiveMatchIds = activePlayerMatchIds.Where(p => notifiedPlayerMatchIds.All(p2 => p2 != p));
                        //var unnotifiedActiveMatchIds = from apm in activePlayerMatchIds
                        //                               from npm in notifiedPlayerMatchIds
                        //                               where notifiedPlayerMatchIds.All(npm => npm != apm)
                        //                               select apm;

                        var unnotifiedMatches = t.Tournament.matches.ToList().Where(m => unnotifiedActiveMatchIds.Contains(m.matchId));
                        //var unnotifiedMatches = from tm in t.Tournament.matches
                        //                        where unnotifiedActiveMatchIds.Contains(tm.matchId)
                        //                        select tm;



                        foreach (var match in unnotifiedMatches)
                        {
                            matchesToNotify.Add(new PlayerNotification(
                                match.matchId,
                                match.playerA.name, 
                                match.playerB.name, 
                                DateTime.Parse(match.starttime.ToString(), null, System.Globalization.DateTimeStyles.RoundtripKind), 
                                match.GetTable().name));

                            p.CalledMatchIds.Add(match.matchId);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex);
            }

            return matchesToNotify;

        }
    }
}
