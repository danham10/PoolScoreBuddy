using CuescoreBuddy.Models.API;

namespace CuescoreBuddy.Services;

public interface IDataStore
{
    public Tournaments Tournaments { get; set; }
}