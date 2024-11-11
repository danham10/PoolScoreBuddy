namespace CuescoreBuddy.Services;

public class DataStore
{
    public int CurrentTournamentId{ get; set; }

    public Tournaments Tournaments { get; set; } = [];
}