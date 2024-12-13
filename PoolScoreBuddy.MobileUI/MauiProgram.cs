using CommunityToolkit.Maui;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using Plugin.LocalNotification;
using Plugin.LocalNotification.AndroidOption;
using PoolScoreBuddy.Domain.Services;
using PoolScoreBuddy.Domain;
using Microsoft.Extensions.Configuration;
using System.Reflection;

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

        builder.Services.AddSingleton<IScoreAPIClient, CueScoreAPIClient>();
        builder.Services.AddSingleton<IDataStore, DataStore>();
        builder.Services.AddSingleton<IMessenger, WeakReferenceMessenger>();

        builder.Services.AddTransient<IPlayerNotificationService, PlayerNotificationService>();

        builder.Services.AddTransient<TournamentPage>();
        builder.Services.AddTransient<TournamentViewModel>();

        builder.Services.AddTransient<TournamentSelectedPage>();
        builder.Services.AddTransient<TournamentSelectedViewModel>();

        builder.Services.AddTransient<PlayerPage>();
        builder.Services.AddTransient<PlayerViewModel>();

        AddHttpClient(builder);

        return builder.Build();
    }

    private static void AddHttpClient(MauiAppBuilder builder)
    {
        var settings = GetAppSettings();

        builder.Services
        .AddHttpClient(Constants.HttpClientName, client =>
        {
            client.BaseAddress = new Uri(Constants.APIBaseUrl);
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", TokenService.GenerateToken());
        })
#if DEBUG
        // Android emulator requires trusted local cert when running in deug
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
;
    }

    private static IConfiguration GetAppSettings()
    {
        //Maui needs some plumbing to implement app settings
        //https://montemagno.com/dotnet-maui-appsettings-json-configuration/
        string appsettingsEnv = "";
#if DEBUG
        appsettingsEnv = "Development";
#else
        appsettingsEnv = "Production";
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
}