using PoolScoreBuddy.API.Domain.Services;

namespace PoolScoreBuddy.API.FunctionApp.Services;

public class EnvironmentSettings : ISettings
{
    public T GetSetting<T>(string key)
    {
        string? value = Environment.GetEnvironmentVariable(key);
        return value == null
            ? throw new ArgumentNullException($"Environment variable {key} not found.")
            : (T)Convert.ChangeType(value, typeof(T));
    }
}