using CoffeeNChillGatewayAPI.DTOs;
using CoffeeNChillGatewayAPI.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CoffeeNChillGatewayAPI.Services
{
    public class AuthService    // rudderz243, 2026
    {
        private readonly UserService _userService;
        private readonly IConfiguration _configuration;

        public AuthService(UserService userService, IConfiguration configuration)
        {
            _userService = userService;
            _configuration = configuration;
        }

        public async Task<bool> RegisterAsync(RegistrationDto newUser)
        {
            string email = newUser.Email.ToLowerInvariant();

            if (await _userService.GetUserByEmailAsync(email) != null)
            {
                return false;
            }

            var user = new UserEntity
            {
                PartitionKey = newUser.Role,
                RowKey = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(newUser.Password),    // Microsoft, 2026af
                FullName = newUser.FullName,
                CreatedDate = DateTime.UtcNow
            };

            await _userService.AddUserAsync(user);

            return true;
        }

        public async Task<AuthResponseDto?> LoginAsync(LoginDto credentials)
        {
            UserEntity? user = await _userService.GetUserByEmailAsync(credentials.Email.ToLowerInvariant());

            if (user == null)
            {
                return null;
            }

            if (!BCrypt.Net.BCrypt.Verify(credentials.Password, user.PasswordHash))
            {
                return null;
            }

            return GenerateToken(user);
        }

        public AuthResponseDto GenerateToken(UserEntity user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiration = DateTime.UtcNow.AddMinutes(double.Parse(jwtSettings["ExpirationMinutes"]!));

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.RowKey),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Email, user.RowKey),
                new Claim(ClaimTypes.Role, user.PartitionKey)
            };

            var token = new JwtSecurityToken(   // Microsoft, 2026ae
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: expiration,
                signingCredentials: credentials);

            return new AuthResponseDto
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Email = user.RowKey,
                FullName = user.FullName,
                Role = user.PartitionKey,
                ExpiresAtUtc = expiration
            };
        }
    }
}
