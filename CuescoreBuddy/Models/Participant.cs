namespace CuescoreBuddy.Models;

public class Participant
{
    public int playerId { get; set; }
    public string name { get; set; }
    public string firstname { get; set; }
    public string lastname { get; set; }
    public string url { get; set; }
    public string image { get; set; }
    public bool isMonitored { get; set; }
}
