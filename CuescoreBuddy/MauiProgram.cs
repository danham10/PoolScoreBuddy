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
                .UseLocalNotification()
            // Initialize the .NET MAUI Community Toolkit by adding the below line of code
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

            builder.Services.AddSingleton<ParticipantPage>();
            builder.Services.AddSingleton<ParticipantViewModel>();

            builder.Services.AddSingleton<TournamentPage>();
            builder.Services.AddSingleton<TournamentViewModel>();

            builder.Services.AddSingleton<IMessenger, WeakReferenceMessenger>();

            return builder.Build();
        }
    }
}
