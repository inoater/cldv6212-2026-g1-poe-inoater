using CoffeeNChillGatewayAPI.DTOs;
using CoffeeNChillGatewayAPI.Models;
using CoffeeNChillGatewayAPI.Services;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace CoffeeNChillGatewayAPI.Tests
{
    public class AuthServiceTests   // Microsoft, 2026ad
    {
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["JwtSettings:SecretKey"] = "CoffeeNChillUnitTestSigningKeyThatIsLongEnough2026!",
                    ["JwtSettings:Issuer"] = "CoffeeNChillGateway",
                    ["JwtSettings:Audience"] = "CoffeeNChillUsers",
                    ["JwtSettings:ExpirationMinutes"] = "60"
                })
                .Build();

            _authService = new AuthService(null!, configuration);
        }

        [Fact]
        public void GenerateToken_ContainsNameEmailAndRoleClaims()
        {
            var user = new UserEntity
            {
                PartitionKey = "Staff",
                RowKey = "barista@coffeenchill.ac.za",
                FullName = "Busi Barista"
            };

            AuthResponseDto result = _authService.GenerateToken(user);
            JwtSecurityToken token = new JwtSecurityTokenHandler().ReadJwtToken(result.Token);     // Microsoft, 2026ae

            Assert.Equal("Busi Barista", token.Claims.First(c => c.Type == ClaimTypes.Name).Value);
            Assert.Equal("barista@coffeenchill.ac.za", token.Claims.First(c => c.Type == ClaimTypes.Email).Value);
            Assert.Equal("Staff", token.Claims.First(c => c.Type == ClaimTypes.Role).Value);
            Assert.Equal("CoffeeNChillGateway", token.Issuer);
        }

        [Fact]
        public void GenerateToken_ExpiresAfterTheConfiguredMinutes()
        {
            AuthResponseDto result = _authService.GenerateToken(new UserEntity { RowKey = "admin@coffeenchill.ac.za" });

            Assert.InRange(result.ExpiresAtUtc, DateTime.UtcNow.AddMinutes(59), DateTime.UtcNow.AddMinutes(61));
        }

        [Fact]
        public void BCryptHash_IsNotThePlainPasswordAndCanBeVerified()
        {
            string hash = BCrypt.Net.BCrypt.HashPassword("Barista@2026");     // Microsoft, 2026af

            Assert.NotEqual("Barista@2026", hash);
            Assert.True(BCrypt.Net.BCrypt.Verify("Barista@2026", hash));
            Assert.False(BCrypt.Net.BCrypt.Verify("WrongPassword1", hash));
        }
    }
}
