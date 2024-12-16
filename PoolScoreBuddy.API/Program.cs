using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using PoolScoreBuddy.API;
using PoolScoreBuddy.API.Endpoints;
using PoolScoreBuddy.API.Services;
using PoolScoreBuddy.Domain.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<Settings>(
    builder.Configuration.GetSection("Settings"));

var settings = builder.Configuration.GetSection("Settings").Get<Settings>();
var secretKey = Encoding.ASCII.GetBytes(settings!.JWTToken);

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

builder.Services.AddHttpClient();
builder.Services.AddSingleton<IScoreAPIClient, CueScoreAPIClient>();
builder.Services.AddSingleton<IScoreClient, ScoreClient>();
builder.Services.AddMemoryCache();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.RegisterTournamentEndpoints();

app.Run();