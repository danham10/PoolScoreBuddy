using PoolScoreBuddy.Domain.Models.API;

namespace PoolScoreBuddy.Domain.Services;

public class DataStore : IDataStore
{
    public Tournaments Tournaments { get; set; } = [];
}
