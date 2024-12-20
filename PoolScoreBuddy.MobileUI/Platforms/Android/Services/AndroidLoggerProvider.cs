using Microsoft.Extensions.Logging;

namespace PoolScoreBuddy;

/// <summary>
/// Native Android logger provider for Microsoft.Extensions.Logging.
/// </summary>
/// <remarks>
/// From https://stackoverflow.com/questions/71599950/logging-on-net-maui
/// </remarks>
public class AndroidLoggerProvider : ILoggerProvider
{
    public AndroidLoggerProvider()
    {
    }

    public ILogger CreateLogger(string categoryName)
    {
        // Category name is often the full class name, like
        // MyApp.ViewModel.MyViewModel
        // This removes the namespace:
        int lastDotPos = categoryName.LastIndexOf('.');
        if (lastDotPos > 0)
        {
            categoryName = categoryName.Substring(lastDotPos + 1);
        }

        return new AndroidLogger(categoryName);
    }

    public void Dispose() { }
}

public class AndroidLogger(string category) : ILogger
{
    private readonly string Category = category;

#pragma warning disable CS8633 // Nullability in constraints for type parameter doesn't match the constraints for type parameter in implicitly implemented interface method'.
    public IDisposable BeginScope<TState>(TState state) => null!;
#pragma warning restore CS8633 // Nullability in constraints for type parameter doesn't match the constraints for type parameter in implicitly implemented interface method'.

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        string message = formatter(state, exception);

        Java.Lang.Throwable? throwable = null;

        if (exception is not null)
        {
            throwable = Java.Lang.Throwable.FromException(exception);
        }

        switch (logLevel)
        {
            case LogLevel.Trace:
                Android.Util.Log.Verbose(Category, throwable!, message);
                break;

            case LogLevel.Debug:
                Android.Util.Log.Debug(Category, throwable!, message);
                break;

            case LogLevel.Information:
                Android.Util.Log.Info(Category, throwable!, message);
                break;

            case LogLevel.Warning:
                Android.Util.Log.Warn(Category, throwable!, message);
                break;

            case LogLevel.Error:
                Android.Util.Log.Error(Category, throwable!, message);
                break;

            case LogLevel.Critical:
                Android.Util.Log.Wtf(Category, throwable!, message);
                break;
        }
    }
}

