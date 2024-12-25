using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PoolScoreBuddy.API.Domain;
using PoolScoreBuddy.API.Domain.Services;
using PoolScoreBuddy.Domain.Services;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();

        services.AddOptions<Settings>()
            .Configure<IConfiguration>((settings, configuration) =>
            {
                configuration.GetSection("Settings").Bind(settings);
            });

        services.AddHttpClient();
        services.AddSingleton<IResilientClientWrapper, ResilientClientWrapper>();
        services.AddSingleton<IScoreAPIClient, CueScoreAPIClient>();
        services.AddSingleton<IScoreClient, ScoreClient>();
    })
    .Build();

host.Run();