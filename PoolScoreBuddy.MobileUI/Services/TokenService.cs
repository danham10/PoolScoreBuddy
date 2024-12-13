using Microsoft.IdentityModel.Tokens;
using PoolScoreBuddy.Domain;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PoolScoreBuddy.Services;
internal class TokenService
{
    internal static string GenerateToken()
    {
        var settings = SettingsResolver.GetSettings();
        var tokenHandler = new JwtSecurityTokenHandler();

        var key = Encoding.ASCII.GetBytes(settings.JWTToken);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(),
            Expires = DateTime.UtcNow.AddMinutes(30),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }
}

