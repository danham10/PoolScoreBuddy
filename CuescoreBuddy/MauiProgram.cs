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
            SetHandlers();

            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .UseLocalNotification()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("Roboto-Regular.ttf", "RobotoRegular");
                    fonts.AddFont("Roboto-Bold.ttf", "Roboto-Bold");
                    
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            builder.Services.AddHttpClient();
            builder.Services.AddTransient<IScoreAPIClient, CueScoreAPIClient>();
            builder.Services.AddSingleton<IDataStore, DataStore>();

            builder.Services.AddSingleton<TournamentPage>();
            builder.Services.AddSingleton<TournamentViewModel>();

            builder.Services.AddSingleton<TournamentSelectedPage>();
            builder.Services.AddSingleton<TournamentSelectedViewModel>();

            builder.Services.AddSingleton<PlayerPage>();
            builder.Services.AddSingleton<PlayerViewModel>();

            builder.Services.AddSingleton<IMessenger, WeakReferenceMessenger>();

            return builder.Build();
        }

        private static void SetHandlers()
        {
            Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping("MyCustomization", (handler, view) =>
            {
                // Hide blue underscore in Entry control
                if (view is Entry)
                {
                    #if ANDROID
                    handler.PlatformView.Background = null;
                    #endif
                }
            });

        }
    }
}
