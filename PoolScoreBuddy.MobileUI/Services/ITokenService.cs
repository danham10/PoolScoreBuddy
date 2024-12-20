namespace PoolScoreBuddy.Services;

internal interface ITokenService
{
    string GenerateToken(string key);
}