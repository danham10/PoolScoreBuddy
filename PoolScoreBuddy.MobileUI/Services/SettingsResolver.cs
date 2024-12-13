using Microsoft.Extensions.Configuration;

namespace PoolScoreBuddy.Services;

public static class SettingsResolver
{
    public static Settings GetSettings()
    {
        var config = ServiceResolver.GetService<IConfiguration>();

        var settings =  config.GetRequiredSection("Settings").Get<Settings>();

        return settings ?? throw new NullReferenceException("Settings are null");
    }
}
