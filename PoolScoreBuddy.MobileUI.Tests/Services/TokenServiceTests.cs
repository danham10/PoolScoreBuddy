using Microsoft.IdentityModel.Tokens;
using PoolScoreBuddy.Services;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Xunit;

namespace PoolScoreBuddy.MobileUI.Test.Services
{
    public class TokenServiceTests
    {
        private readonly TokenService _tokenService;

        public TokenServiceTests()
        {
            _tokenService = new TokenService();
        }

        [Fact]
        public void GenerateToken_ValidKey_ReturnsToken()
        {
            // Arrange
            var key = "supersecretkeyintheworld1234567890";

            // Act
            var token = _tokenService.GenerateToken(key);

            // Assert
            Assert.False(string.IsNullOrEmpty(token));
        }

        [Fact]
        public void GenerateToken_ValidKey_TokenIsValid()
        {
            // Arrange
            var key = "supersecretkeyintheworld1234567890";
            var token = _tokenService.GenerateToken(key);
            var tokenHandler = new JwtSecurityTokenHandler();
            var encodedKey = Encoding.ASCII.GetBytes(key);

            // Act
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(encodedKey),
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);

            // Assert
            Assert.NotNull(principal);
            Assert.NotNull(validatedToken);
        }

        [Fact]
        public void GenerateToken_InvalidKey_ThrowsException()
        {
            // Arrange
            var key = "tooshort";

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => _tokenService.GenerateToken(key));
        }
    }
}

