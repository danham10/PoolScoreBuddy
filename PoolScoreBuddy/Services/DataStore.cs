using PoolScoreBuddy.Models.API;

namespace PoolScoreBuddy.Services;

public class DataStore : IDataStore
{
    public Tournaments Tournaments { get; set; } = [];
}
