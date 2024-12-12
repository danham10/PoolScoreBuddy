using PoolScoreBuddy.Domain.Models.API;

namespace PoolScoreBuddy.Repository;

public class DataStore : IDataStore
{
    public Tournaments Tournaments { get; set; } = [];
}
