namespace CuescoreBuddy
{
    public partial class AppShell : Shell
    {
        public Dictionary<string, Type> Routes { get; private set; } = [];

        public AppShell()
        {
            InitializeComponent();
            Routes.Add(nameof(TournamentPage), typeof(TournamentPage));
            Routes.Add(nameof(TournamentSelectedPage), typeof(TournamentSelectedPage));
            Routes.Add(nameof(PlayerPage), typeof(PlayerPage));

            foreach (var route in Routes)
            {
                Routing.RegisterRoute(route.Key, route.Value);
            }
        }
    }
}
