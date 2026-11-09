using CoffeeNChillGatewayAPI.Configuration;
using CoffeeNChillGatewayAPI.DTOs;
using CoffeeNChillGatewayAPI.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

namespace CoffeeNChillGatewayAPI.Services
{
    public class TokenService   // Microsoft, 2026af; Jones, Bradley and Sakimura, 2015; Auth0, 2026; rudderz243, 2026
    {
        private readonly JwtSettings _jwtSettings;
        private readonly JsonWebTokenHandler _tokenHandler = new JsonWebTokenHandler();

        public TokenService(IOptions<JwtSettings> jwtOptions)
        {
            _jwtSettings = jwtOptions.Value;
        }

        public static SymmetricSecurityKey CreateSigningKey(string secretKey)
        {
            return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        }

        public AuthResponseDto GenerateToken(UserEntity user)
        {
            DateTime expiresAtUtc = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.RowKey),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Email, user.RowKey),
                new Claim(ClaimTypes.Role, user.PartitionKey)
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                IssuedAt = DateTime.UtcNow,
                NotBefore = DateTime.UtcNow,
                Expires = expiresAtUtc,
                SigningCredentials = new SigningCredentials(
                    CreateSigningKey(_jwtSettings.SecretKey),
                    SecurityAlgorithms.HmacSha256)
            };

            string token = _tokenHandler.CreateToken(tokenDescriptor);

            return new AuthResponseDto
            {
                Token = token,
                TokenType = "Bearer",
                ExpiresAtUtc = expiresAtUtc,
                Email = user.RowKey,
                FullName = user.FullName,
                Role = user.PartitionKey
            };
        }
    }
}
