using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using PoolScoreBuddy.API.Endpoints;
using PoolScoreBuddy.API.Services;
using PoolScoreBuddy.Di;
using PoolScoreBuddy.Domain;
using PoolScoreBuddy.Domain.Models;
using PoolScoreBuddy.Domain.Services;

var builder = WebApplication.CreateBuilder(args);

var secretKey = ApiSettings.GenerateSecretByte();

// Add services to the container.
builder.Services.AddAuthentication(config =>
{
    config.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    config.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(config =>
{
    config.RequireHttpsMetadata = false;
    config.SaveToken = true;
    config.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(secretKey),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});

builder.Services.AddAuthorization();

builder.Services.AddHttpClient(ApiProviderType.CueScore.ToString(), client =>
{
    client.BaseAddress = new Uri(Constants.CueScoreBaseUrl);
});

builder.Services.AddMemoryCache();
builder.Services.AddSingleton<IScoreAPIClient, CueScoreAPIClient>(); //Singleton - need to track bad endpoints

builder.Services.Configure<Settings>(
    builder.Configuration.GetSection("Settings"));

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IScoreClient, ScoreClient>();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

//app.UseHsts();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.RegisterTournamentEndpoints();

app.Run();