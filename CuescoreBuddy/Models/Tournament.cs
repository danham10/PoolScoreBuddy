using System.ComponentModel;
using System.Text.Json;

namespace CuescoreBuddy.Models;

//TODO proper case this and change deserializer to ignore case

/// <summary>
/// Generated from special paste of JSON from https://api.cuescore.com/
/// </summary>
public partial class Tournament
{
    public int tournamentId { get; set; }
    public string name { get; set; }
    public int type { get; set; }
    public int format { get; set; }
    public string url { get; set; }
    public string timezone { get; set; }
    public string displayDate { get; set; }
    public DateTime starttime { get; set; }
    public DateTime stoptime { get; set; }
    public string deadline { get; set; }
    public string status { get; set; }
    public string discipline { get; set; }
    public Contact[] contacts { get; set; }
    public object owner { get; set; }
    public object[] venues { get; set; }
    public object[] standings { get; set; }
    public Match[] matches { get; set; }
}

public class TournamentFacade
{
    private List<Player>? _players;
    public Tournament Tournament { get; private set; }
    public List<MonitoredPlayer> MonitoredPlayers { get; set; } = [];
    public bool IsBusy { get; private set; }

    public TournamentFacade(int tournamentId)
    {
        Tournament = new Tournament() { tournamentId = tournamentId };
    }

    public TournamentFacade(Tournament tournament)
    {
        Tournament = tournament;
    }

    public async Task Fetch(ICueScoreService cueScoreService, int tournamentId)
    {
        Tournament = await cueScoreService.GetTournament(tournamentId);
    }

    public MonitoredPlayer? TogglePlayerEnabled(int playerId)
    {
        var monitoredPlayer = MonitoredPlayers.FirstOrDefault(p => p.PlayerId == playerId);

        if (monitoredPlayer == null)
        {
            monitoredPlayer = new MonitoredPlayer() { PlayerId = playerId, CreateDate = DateTime.UtcNow };
            MonitoredPlayers.Add(monitoredPlayer);

            return monitoredPlayer;
        } 
        else
        {
            MonitoredPlayers.Remove(monitoredPlayer);
            return null;
        }
    }
    public async Task LoadPlayers(ICueScoreService cueScoreService)
    {
        if (_players == null)
        {
            IsBusy = true;
            _players = await cueScoreService.GetPlayers(Tournament.tournamentId);
            IsBusy = false;
        }

        //TODO currently refresh will blat the notified matches. Prob best to store those in separate class
    }

    public IEnumerable<Player> GetLoadedPlayers()
    {
        if (_players == null)
            throw new InvalidOperationException("Must call LoadPlayers first");

        var players = from p in _players
               join mp in MonitoredPlayers on p.playerId equals mp.PlayerId
               into MonitoredPlayersGroup
               from mp in MonitoredPlayersGroup.DefaultIfEmpty()
               select new Player()
               {
                   playerId = p.playerId,
                   firstname = p.firstname,
                   lastname = p.lastname,
                   name = p.name,
                   image = p.image,
                   MonitoredPlayer = mp,
               };

        return players.OrderBy(p => p.name);
    }

    public Player? GetPlayerById(string id)
    {
        return _players?.FirstOrDefault(s => s.playerId == Convert.ToInt32(id));
    }

    public bool IsFinished() => Tournament.status == "Finished";
    public List<Match> PlayerMatches(int playerId)
    {
        return Tournament.matches.ToList().Where(m => m.playerA.playerId == playerId || m.playerB.playerId == playerId).ToList();
    }

    public List<Match> ActivePlayerMatches(int playerId)
    {
        return PlayerMatches(playerId).Where(m => m.matchstatusCode == 1).ToList();
    }




};



public class Owner
{
    public int organizationId { get; set; }
    public string stub { get; set; }
    public string name { get; set; }
    public string url { get; set; }
    public string logo { get; set; }
    public string profileColor { get; set; }
}

public class Contact
{
    public int playerId { get; set; }
    public string name { get; set; }
    public string firstname { get; set; }
    public string lastname { get; set; }
    public string url { get; set; }
    public string image { get; set; }
}
public class Match
{
    public int matchId { get; set; }
    public int matchno { get; set; }
    public string roundName { get; set; }
    public int round { get; set; }
    public Player playerA { get; set; }
    public Player playerB { get; set; }
    public int scoreA { get; set; }
    public int scoreB { get; set; }
    public int raceTo { get; set; }
    public string discipline { get; set; }
    public int branch { get; set; }
    public int penalty { get; set; }
    public int groupNo { get; set; }
    public int inningsPlayerA { get; set; }
    public int inningsPlayerB { get; set; }
    public string highBreaksA { get; set; }
    public string highBreaksB { get; set; }
    public int runoutsA { get; set; }
    public int runoutsB { get; set; }
    public object table { get; set; }
    public int tournamentId { get; set; }
    public object starttime { get; set; }
    public object stoptime { get; set; }
    public string matchstatus { get; set; }
    public int matchstatusCode { get; set; }
    public int bestOfSets { get; set; }
    public bool useInnings { get; set; }
    public object[] frames { get; set; }
    public object properties { get; set; }
    public object[] sets { get; set; }
    public int curVersion { get; set; }
    public object[] notes { get; set; }
    public int matchType { get; set; }
    public string videoLink { get; set; }
    public string comment { get; set; }
    public object winnerNext { get; set; }
    public object loserNext { get; set; }

    /// <summary>
    /// Table returns inconsistent data. Either empty array or a Table object.
    /// </summary>
    /// <returns></returns>
    public Table? GetTable()
    {
        try
        {
            return JsonSerializer.Deserialize<Table>(table.ToString());
        }
        catch (Exception)
        {
            return new Table();
        }
    }
}

public partial class Player
{
    public int playerId { get; set; }
    public string name { get; set; }
    public string firstname { get; set; }
    public string lastname { get; set; }
    public string url { get; set; }
    public string image { get; set; }
}

public partial class Player
{
    public bool IsMonitored { get => MonitoredPlayer != null; }
    public MonitoredPlayer? MonitoredPlayer { get; set; }
}

public class MonitoredPlayer
{
    public DateTime CreateDate { get; set; }
    public int PlayerId { get; set; }
    public List<int> CalledMatchIds = [];
    public List<int> ResultsMatchIds = []; //TODO implement
}

public class Table
{
    public int tableId { get; set; }
    public string name { get; set; }
    public string manufacturer { get; set; }
    public string model { get; set; }
    public string branch { get; set; }
    public int branchId { get; set; }
    public int size { get; set; }
    public string cloth { get; set; }
    public string pockets { get; set; }
    public string slate { get; set; }
    public string description { get; set; }
}
