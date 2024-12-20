using Microsoft.Extensions.Configuration;

namespace PoolScoreBuddy.Services;

public class SettingsResolver : ISettingsResolver
{
    public Settings GetSettings()
    {
        var config = ServiceResolver.GetService<IConfiguration>();

        var settings =  config.GetRequiredSection("Settings").Get<Settings>();

        return settings ?? throw new NullReferenceException("Settings are null");
    }
}

public interface ISettingsResolver
{
    public Settings GetSettings();
}