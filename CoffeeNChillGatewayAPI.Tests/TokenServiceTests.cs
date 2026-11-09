using CoffeeNChillGatewayAPI.Configuration;
using CoffeeNChillGatewayAPI.DTOs;
using CoffeeNChillGatewayAPI.Models;
using CoffeeNChillGatewayAPI.Services;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace CoffeeNChillGatewayAPI.Tests
{
    public class TokenServiceTests  // Microsoft, 2026ae; Microsoft, 2026af
    {
        private readonly JwtSettings _jwtSettings = new JwtSettings
        {
            SecretKey = "Unit-Test-Signing-Key-That-Is-Long-Enough-2026",
            Issuer = "CoffeeNChillGateway",
            Audience = "CoffeeNChillClients",
            ExpirationMinutes = 30
        };

        private readonly UserEntity _staffUser = new UserEntity
        {
            PartitionKey = "Staff",
            RowKey = "barista@coffeenchill.ac.za",
            FullName = "Busi Barista"
        };

        [Fact]
        public async Task GenerateToken_ContainsNameEmailAndRoleClaims()
        {
            var tokenService = new TokenService(Options.Create(_jwtSettings));

            AuthResponseDto authResponse = tokenService.GenerateToken(_staffUser);

            TokenValidationResult validationResult = await ValidateAsync(authResponse.Token, _jwtSettings.SecretKey);

            Assert.True(validationResult.IsValid);
            Assert.Equal("Busi Barista", validationResult.ClaimsIdentity.FindFirst(ClaimTypes.Name)?.Value);
            Assert.Equal("barista@coffeenchill.ac.za", validationResult.ClaimsIdentity.FindFirst(ClaimTypes.Email)?.Value);
            Assert.Equal("Staff", validationResult.ClaimsIdentity.FindFirst(ClaimTypes.Role)?.Value);
        }

        [Fact]
        public void GenerateToken_ReturnsBearerTokenThatExpiresAtConfiguredTime()
        {
            var tokenService = new TokenService(Options.Create(_jwtSettings));

            AuthResponseDto authResponse = tokenService.GenerateToken(_staffUser);

            Assert.Equal("Bearer", authResponse.TokenType);
            Assert.Equal("Staff", authResponse.Role);
            Assert.InRange(
                authResponse.ExpiresAtUtc,
                DateTime.UtcNow.AddMinutes(29),
                DateTime.UtcNow.AddMinutes(31));
        }

        [Fact]
        public async Task GenerateToken_IsRejectedWhenCheckedWithADifferentKey()
        {
            var tokenService = new TokenService(Options.Create(_jwtSettings));

            AuthResponseDto authResponse = tokenService.GenerateToken(_staffUser);

            TokenValidationResult validationResult =
                await ValidateAsync(authResponse.Token, "A-Completely-Different-Signing-Key-For-Testing-2026");

            Assert.False(validationResult.IsValid);
        }

        private Task<TokenValidationResult> ValidateAsync(string token, string signingKey)
        {
            var tokenHandler = new JsonWebTokenHandler();

            return tokenHandler.ValidateTokenAsync(token, new TokenValidationParameters
            {
                ValidIssuer = _jwtSettings.Issuer,
                ValidAudience = _jwtSettings.Audience,
                IssuerSigningKey = TokenService.CreateSigningKey(signingKey),
                ValidateLifetime = true
            });
        }
    }
}
