namespace CuescoreBuddy.Services;

public static class ServiceResolver
{
    #pragma warning disable CS8602, CS8603 // Dereference of a possibly null reference., Possible null reference return.
    public static T GetService<T>() => IPlatformApplication.Current.Services.GetService<T>();
}
