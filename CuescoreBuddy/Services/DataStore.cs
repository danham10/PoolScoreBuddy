using CuescoreBuddy.Models.API;

namespace CuescoreBuddy.Services;

public class DataStore : IDataStore
{
    public Tournaments Tournaments { get; set; } = [];
}

public interface IDataStore
{
    public Tournaments Tournaments { get; set; }
}