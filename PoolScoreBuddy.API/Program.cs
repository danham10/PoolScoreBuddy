using PoolScoreBuddy.API.Endpoints;
using PoolScoreBuddy.API.Services;
using PoolScoreBuddy.Di;
using PoolScoreBuddy.Domain;
using PoolScoreBuddy.Domain.Models;
using PoolScoreBuddy.Domain.Services;
using System.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddHttpClient(ApiProviderType.CueScore.ToString(), client =>
{
    client.BaseAddress = new Uri(Constants.CueScoreBaseUrl);
});

builder.Services.AddMemoryCache();
builder.Services.AddTransient<IScoreAPIClient, CueScoreAPIClient>();

builder.Services.Configure<Settings>(
    builder.Configuration.GetSection("Settings"));

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IScoreClient, ScoreClient>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.RegisterTournamentEndpoints();

app.Run();