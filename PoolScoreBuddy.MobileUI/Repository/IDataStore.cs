using PoolScoreBuddy.Domain.Models.API;

namespace PoolScoreBuddy.Repository;

public interface IDataStore
{
    public Tournaments Tournaments { get; set; }
}