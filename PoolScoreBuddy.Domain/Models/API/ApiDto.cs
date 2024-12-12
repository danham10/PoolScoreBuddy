﻿namespace PoolScoreBuddy.Domain.Models.API;

public record TournamentDto() : ApiDto
{
    public int TournamentId { get; set; }
    public IEnumerable<int>? PlayerIds { get; set; }
}

public record PlayersDto : ApiDto
{
    public int TournamentId { get; set; }
}

public record ApiDto
{
    public List<string> BaseAddresses { get; set; } = [];
}
