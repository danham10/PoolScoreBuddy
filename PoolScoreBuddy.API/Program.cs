using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using PoolScoreBuddy.API.Endpoints;
using PoolScoreBuddy.API.Domain.Services;
using PoolScoreBuddy.Domain.Services;
using System.Text;
using PoolScoreBuddy.API.Services;

var builder = WebApplication.CreateBuilder(args);


var jWTTokenSetting = builder.Configuration["JWTToken"]!;
var secretKey = Encoding.ASCII.GetBytes(jWTTokenSetting);

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
builder.Services.AddSingleton<IResilientClientWrapper, ResilientClientWrapper>();
builder.Services.AddSingleton<IScoreAPIClient, CueScoreAPIClient>();
builder.Services.AddSingleton<IScoreClient, ScoreClient>();
builder.Services.AddSingleton<ISettings, AppSettings>();
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