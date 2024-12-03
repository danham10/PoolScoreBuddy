using PoolScoreBuddy.Domain.Models.API;

namespace PoolScoreBuddy.Domain.Services;

public interface IDataStore
{
    public Tournaments Tournaments { get; set; }
}