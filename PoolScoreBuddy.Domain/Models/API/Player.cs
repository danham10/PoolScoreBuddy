﻿namespace PoolScoreBuddy.Domain.Models.API;
public partial class Player
{
    private int playerId;

    public int PlayerId { get => playerId == 0 ? TeamId : playerId; set => playerId = value; }

    public int TeamId { get; set; }
    public string Name { get; set; } = null!;
    public string? Firstname { get; set; }
    public string? Lastname { get; set; }
    public string? Url { get; set; }
    public string? Image { get; set; }
}

public partial class Player
{
    public bool IsMonitored { get => MonitoredPlayer != null; }
    public bool MonitoringEnabled{ get; set; }
    public MonitoredPlayer? MonitoredPlayer { get; set; }
}

public class MonitoredPlayer
{
    public DateTime CreateDate { get; set; }
    public int PlayerId { get; set; }
    public List<int> CalledMatchIds = [];
    public List<int> ResultsMatchIds = [];
}
