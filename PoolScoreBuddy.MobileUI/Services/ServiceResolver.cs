namespace PoolScoreBuddy.Services;

public static class ServiceResolver
{
    public static T GetService<T>() => IPlatformApplication.Current!.Services.GetService<T>()!;
}
