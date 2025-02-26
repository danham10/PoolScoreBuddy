using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PoolScoreBuddy.API.Domain;
using PoolScoreBuddy.API.Domain.Services;
using PoolScoreBuddy.API.FunctionApp.Services;
using PoolScoreBuddy.Domain.Services;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services =>
    {
        services.AddOptions<Settings>()
            .Configure<IConfiguration>((settings, configuration) =>
            {
                configuration.GetSection("Settings").Bind(settings);
            });

        services.AddHttpClient();
        services.AddSingleton<IResilientClientWrapper, ResilientClientWrapper>();
        services.AddSingleton<IScoreAPIClient, CueScoreAPIClient>();
        services.AddSingleton<IScoreClient, ScoreClient>();
        services.AddSingleton<ISettings, EnvironmentSettings>();
    })
    .Build();

host.Run();