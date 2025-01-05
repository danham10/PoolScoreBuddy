namespace PoolScoreBuddy.API.Domain.Services;

public interface ISettings
{
    public T GetSetting<T>(string key);
}
