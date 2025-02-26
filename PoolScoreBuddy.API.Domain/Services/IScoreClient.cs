﻿using PoolScoreBuddy.Domain.Models.API;

namespace PoolScoreBuddy.API.Domain.Services
{
    public interface IScoreClient
    {
        public Task<Tournament> GetTournament(int id, int[]? playerIds, int[]? notifiedMatchIds);
        public Task<IEnumerable<Player>> GetPlayers(int tournamentId);
    }
}