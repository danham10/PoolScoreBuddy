namespace CuescoreBuddy
{
    public partial class AppShell : Shell
    {
        public Dictionary<string, Type> Routes { get; private set; } = [];

        public AppShell()
        {
            InitializeComponent();
            Routes.Add(nameof(AboutPage), typeof(AboutPage));
            Routes.Add(nameof(ParticipantPage), typeof(ParticipantPage));

            foreach (var route in Routes)
            {
                Routing.RegisterRoute(route.Key, route.Value);
            }
        }
    }
}
