using PoolScoreBuddy.Models.API;

namespace PoolScoreBuddy.Services;

public interface IDataStore
{
    public Tournaments Tournaments { get; set; }
}