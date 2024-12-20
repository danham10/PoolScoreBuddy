﻿namespace PoolScoreBuddy.Domain.Models.API;

public class Tournaments : List<ITournamentDecorator>, ITournaments
{
    public ITournamentDecorator GetTournamentById(int tournamentId)
    {
        var tournament = this.FirstOrDefault(t => t.Tournament.TournamentId == tournamentId);

        tournament ??= new TournamentDecorator(tournamentId);

        return tournament;
    }

    public void AddIfMissing(ITournamentDecorator tournament)
    {
        var exists = (from t in this
                      where t.Tournament.TournamentId == tournament.Tournament.TournamentId
                      select t)
                      .Any();

        if (!exists)
        {
            Add(tournament);
        }
    }

    public bool ShouldMonitor() => ActiveTournaments() && AnyMonitoredPlayers();

    public bool ActiveTournaments()
    {
        var x = this.All(t => t.Tournament != null && t.IsFinished()) == false;
        return x;
    }

    public bool AnyMonitoredPlayers()
    {
        var x =
            (from t in this
             from mp in t.MonitoredPlayers
             select mp)
            .Any();

        return x;
    }

    public void CancelMonitoredPlayers() => ForEach(t => t.MonitoredPlayers = []);
}
