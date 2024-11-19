using CommunityToolkit.Maui;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using Plugin.LocalNotification;

namespace CuescoreBuddy
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .UseLocalNotification()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            builder.Services.AddHttpClient();
            builder.Services.AddTransient<ICueScoreService, CueScoreService>();
            builder.Services.AddSingleton<DataStore>();

            builder.Services.AddSingleton<TournamentPage>();
            builder.Services.AddSingleton<TournamentViewModel>();

            builder.Services.AddSingleton<TournamentSelectedPage>();
            builder.Services.AddSingleton<TournamentSelectedViewModel>();

            builder.Services.AddSingleton<PlayerPage>();
            builder.Services.AddSingleton<PlayerViewModel>();

            builder.Services.AddSingleton<IMessenger, WeakReferenceMessenger>();

            return builder.Build();
        }
    }
}
