using PoolScoreBuddy.API.Domain.Services;

namespace PoolScoreBuddy.API.Services;

public class AppSettings(IConfiguration configuration) : ISettings
{
    public T GetSetting<T>(string key)
    {
        string? value = configuration[key];
        return value == null
            ? throw new ArgumentNullException($"Setting {key} not found.")
            : (T)Convert.ChangeType(value, typeof(T));
    }
}