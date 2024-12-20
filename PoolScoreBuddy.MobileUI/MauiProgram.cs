using CommunityToolkit.Maui;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using Plugin.LocalNotification;
using Plugin.LocalNotification.AndroidOption;
using PoolScoreBuddy.Domain.Services;
using PoolScoreBuddy.Domain;
using Microsoft.Extensions.Configuration;
using System.Reflection;
using PoolScoreBuddy.Domain.Models.API;

namespace PoolScoreBuddy;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
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

        builder.Services.AddLogging(
            configure =>
            {

#if ANDROID
#if DEBUG
                LogLevel androidLogLevel = LogLevel.Debug;
#else
        LogLevel androidLogLevel = LogLevel.Information;
#endif

                configure
                    .AddProvider(new AndroidLoggerProvider())
                    .AddFilter("PoolScoreBuddy", androidLogLevel);
#endif

            }
        );

        builder.Services.AddSingleton<INotificationsChallenger, NotificationsChallenger>();
        builder.Services.AddSingleton<ITokenService, TokenService>();
        builder.Services.AddSingleton<ISettingsResolver,  SettingsResolver>();
        builder.Services.AddSingleton<IPoolAppShell, PoolAppShell>();
        builder.Services.AddSingleton<IAlert, Alert>();
        builder.Services.AddSingleton<IEnsureConnectivity, EnsureConnectivity>();
        builder.Services.AddSingleton<IScoreAPIClient, CueScoreAPIClient>();
        builder.Services.AddSingleton<IDataStore, DataStore>();
        builder.Services.AddSingleton<IMessenger, WeakReferenceMessenger>();
        builder.Services.AddSingleton<IPlayerNotificationService, PlayerNotificationService>();

        builder.Services.AddTransient<ITournamentDecorator, TournamentDecorator>();

        builder.Services.AddTransient<TournamentPage>();
        builder.Services.AddTransient<TournamentViewModel>();

        builder.Services.AddTransient<TournamentSelectedPage>();
        builder.Services.AddTransient<TournamentSelectedViewModel>();

        builder.Services.AddTransient<PlayerPage>();
        builder.Services.AddTransient<PlayerViewModel>();

        AddHttpClient(builder);
        SetUiHandlers();

        return builder.Build();
    }  

    private static void AddHttpClient(MauiAppBuilder builder)
    {
        var settings = new Settings();
        builder.Configuration.GetRequiredSection(nameof(Settings)).Bind(settings);
        var tokenService = builder.Services.BuildServiceProvider().GetService<ITokenService>();

        builder.Services
        .AddHttpClient(Constants.HttpClientName, client =>
        {
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenService!.GenerateToken(settings.JWTToken));
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

    private static void SetUiHandlers()
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