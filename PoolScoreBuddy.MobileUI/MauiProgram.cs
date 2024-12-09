using CommunityToolkit.Maui;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using Plugin.LocalNotification;
using Plugin.LocalNotification.AndroidOption;
using PoolScoreBuddy.Domain.Services;
using PoolScoreBuddy.Domain;
using Microsoft.Extensions.Configuration;
using System.Reflection;
using Polly;
using Polly.Retry;
using Polly.Timeout;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Options;
using PoolScoreBuddy.Domain.Models;

namespace PoolScoreBuddy;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        SetHandlers();

        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "Quicksand-Regular");
                fonts.AddFont("Quicksand-Bold.ttf", "Quicksand-Bold");

            })
            .UseLocalNotification(static config =>
            {
                config.AddAndroid(android =>
                {
                    android.AddChannel(new NotificationChannelRequest
                    {
                        Id = Constants.ChannelId
                    });
                });
            });

        builder.Configuration.AddConfiguration(GetAppSettings());

#if DEBUG
        builder.Logging.AddDebug();
#endif




        //builder.Services.AddHttpClient();
        builder.Services.AddTransient<IScoreAPIClient, CueScoreAPIClient>();
        builder.Services.AddTransient<IPlayerNotificationService, PlayerNotificationService>();
        builder.Services.AddSingleton<IDataStore, DataStore>();

        builder.Services.AddTransient<TournamentPage>();
        builder.Services.AddTransient<TournamentViewModel>();

        builder.Services.AddTransient<TournamentSelectedPage>();
        builder.Services.AddTransient<TournamentSelectedViewModel>();

        builder.Services.AddTransient<PlayerPage>();
        builder.Services.AddTransient<PlayerViewModel>();

        builder.Services.AddSingleton<IMessenger, WeakReferenceMessenger>();

        //builder.Services.AddTransient<LoggingDelegatingHandler>();

        AddHttpClients(builder);

        builder.Services.AddResiliencePipeline(Constants.ResiliencePipelineKey, static builder =>
        {
            // See: https://www.pollydocs.org/strategies/retry.html
            builder.AddRetry(new RetryStrategyOptions
            {
                ShouldHandle = new PredicateBuilder().Handle<TimeoutRejectedException>()
            });

            // See: https://www.pollydocs.org/strategies/timeout.html
            builder.AddTimeout(TimeSpan.FromSeconds(1.5));
        });

        return builder.Build();
    }

    private static void AddHttpClients(MauiAppBuilder builder)
    {
        var settings = GetAppSettings();


        builder.Services.AddHttpClient(ApiProviderType.CueScore.ToString(), client =>
        {
            client.BaseAddress = new Uri(Constants.CueScoreBaseUrl);
        });

        builder.Services
        .AddHttpClient(ApiProviderType.CueScoreProxy.ToString(), client =>
        {
            client.BaseAddress = new Uri(Constants.APIBaseUrl);
        })
#if DEBUG
        // Android emulator requires untrusted local cert when running in deug
        //https://learn.microsoft.com/en-us/previous-versions/xamarin/cross-platform/deploy-test/connect-to-local-web-services#bypass-the-certificate-security-check
        .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
        {
            ClientCertificateOptions = ClientCertificateOption.Manual,
            ServerCertificateCustomValidationCallback =
        (httpRequestMessage, cert, cetChain, policyErrors) =>
        {
            return true;
        }
        })
#endif
        .AddResilienceHandler("MyResilienceStrategy", resilienceBuilder => // Adds resilience policy named "MyResilienceStrategy"
        {
            // Retry Strategy configuration
            resilienceBuilder.AddRetry(new HttpRetryStrategyOptions // Configures retry behavior
            {
                MaxRetryAttempts = 4, // Maximum retries before throwing an exception (default: 3)

                Delay = TimeSpan.FromSeconds(2), // Delay between retries (default: varies by strategy)

                BackoffType = DelayBackoffType.Exponential, // Exponential backoff for increasing delays (default)

                UseJitter = true, // Adds random jitter to delay for better distribution (default: false)

                ShouldHandle = new PredicateBuilder<HttpResponseMessage>() // Defines exceptions to trigger retries
                .Handle<HttpRequestException>() // Includes any HttpRequestException
                .HandleResult(response => !response.IsSuccessStatusCode) // Includes non-successful responses
            });

            // Timeout Strategy configuration
            resilienceBuilder.AddTimeout(TimeSpan.FromSeconds(5)); // Sets a timeout limit for requests (throws TimeoutRejectedException)

            // Circuit Breaker Strategy configuration
            resilienceBuilder.AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions // Configures circuit breaker behavior
            {
                // Tracks failures within this time frame
                SamplingDuration = TimeSpan.FromSeconds(10),

                // Trips the circuit if failure ratio exceeds this within sampling duration (20% failures allowed)
                FailureRatio = 0.2,

                // Requires at least this many successful requests within sampling duration to reset
                MinimumThroughput = 3,

                // How long the circuit stays open after tripping
                BreakDuration = TimeSpan.FromSeconds(1),

                // Defines exceptions to trip the circuit breaker
                ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                .Handle<HttpRequestException>() // Includes any HttpRequestException
                .HandleResult(response => !response.IsSuccessStatusCode) // Includes non-successful responses
            });
        });
    }

    private static IConfiguration GetAppSettings()
    {
        //Maui needs some plumbing to implement app settings
        //https://montemagno.com/dotnet-maui-appsettings-json-configuration/
        string appsettingsEnv = "";
#if DEBUG
        appsettingsEnv = "Development";
#else
        appsettingsEnv = "Production"
#endif
        string path = FileSystem.AppDataDirectory;
        var a = Assembly.GetExecutingAssembly();
        Stream stream = a.GetManifestResourceStream($"PoolScoreBuddy.appsettings.{appsettingsEnv}.json")!;

        return new ConfigurationBuilder()
            .AddJsonStream(stream)
            .Build();
    }

    private static void SetHandlers()
    {
        Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping("EntryUnderscoreHide", (handler, view) =>
        {
            if (view is Entry)
            {
#if ANDROID
                handler.PlatformView.Background = null;
#endif
            }
        });

    }


    /// <summary>
    /// Android emulator requires untrusted local cert running in deug
    /// </summary>
    /// <returns></returns>
    /// <remarks>
    //https://learn.microsoft.com/en-us/previous-versions/xamarin/cross-platform/deploy-test/connect-to-local-web-services#bypass-the-certificate-security-check
    /// </remarks>
    private static HttpClientHandler GetInsecureHandler()
    {
        HttpClientHandler handler = new()
        {
            ServerCertificateCustomValidationCallback =
            (message, cert, chain, errors) =>
            {
                if (cert!.Issuer.Equals("CN=localhost"))
                    return true;
                return errors == System.Net.Security.SslPolicyErrors.None;
            }
        };
        return handler;
    }
}

//public class LoggingDelegatingHandler(ILogger<LoggingDelegatingHandler> logger)
//    : DelegatingHandler
//{
//    protected override async Task<HttpResponseMessage> SendAsync(
//        HttpRequestMessage request,
//        CancellationToken cancellationToken)
//    {
//        try
//        {
//            logger.LogInformation("Before HTTP request");

//            var result = await base.SendAsync(request, cancellationToken);

//            result.EnsureSuccessStatusCode();

//            logger.LogInformation("After HTTP request");

//            return result;
//        }
//        catch (Exception e)
//        {
//            logger.LogError(e, "HTTP request failed");

//            throw;
//        }
//    }
//}