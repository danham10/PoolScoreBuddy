using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using CommunityToolkit.Mvvm.Messaging;
using CuescoreBuddy.Platforms;

namespace CuescoreBuddy
{
    [Activity(Theme = "@style/Maui.SplashTheme", LaunchMode = LaunchMode.SingleTop, MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        private Intent? _startServiceIntent;

        public MainActivity()
        {
            var messenger = MauiApplication.Current.Services.GetService<IMessenger>();

            messenger.Register<CuescoreBackgroundChecker>(this, (recipient, message) =>
            {
                var serviceIntent = new Intent(this, typeof(CuescoreCheckerService));
                StartService(serviceIntent);
            });
        }

        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState); //TODO remove?
        }
    }
}
