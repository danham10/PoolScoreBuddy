using Microsoft.Extensions.Configuration;

namespace PoolScoreBuddy.Domain;

public static class SettingsResolver
{
    public static Settings GetSettings()
    {
        var config = ServiceResolver.GetService<IConfiguration>();

        var settings =  config.GetRequiredSection("Settings").Get<Settings>();

        if (settings == null)
            throw new NullReferenceException("Settings are null");

        return settings;
    }
}
