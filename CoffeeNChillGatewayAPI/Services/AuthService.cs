using Azure;
using CoffeeNChillGatewayAPI.Constants;
using CoffeeNChillGatewayAPI.Configuration;
using CoffeeNChillGatewayAPI.DTOs;
using CoffeeNChillGatewayAPI.Models;

namespace CoffeeNChillGatewayAPI.Services
{
    public class AuthService    // rudderz243, 2026; BcryptNet, 2026
    {
        private const int BcryptWorkFactor = 12;

        private static readonly string _dummyPasswordHash =
            BCrypt.Net.BCrypt.HashPassword("CoffeeNChill-Dummy-Password", BcryptWorkFactor);

        private readonly UserService _userService;
        private readonly TokenService _tokenService;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            UserService userService,
            TokenService tokenService,
            ILogger<AuthService> logger)
        {
            _userService = userService;
            _tokenService = tokenService;
            _logger = logger;
        }

        public static string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password, BcryptWorkFactor);  // OWASP Foundation, 2026a
        }

        public static bool VerifyPassword(string password, string passwordHash)
        {
            return BCrypt.Net.BCrypt.Verify(password, passwordHash);
        }

        public async Task<UserEntity?> RegisterAsync(RegisterRequestDto newUser)
        {
            string email = newUser.Email.Trim().ToLowerInvariant();

            if (await _userService.GetUserByEmailAsync(email) != null)
            {
                return null;
            }

            string role = Roles.All.First(
                r => string.Equals(r, newUser.Role, StringComparison.OrdinalIgnoreCase));

            var userEntity = new UserEntity
            {
                PartitionKey = role,
                RowKey = email,
                PasswordHash = HashPassword(newUser.Password),
                FullName = newUser.FullName.Trim(),
                CreatedDate = DateTime.UtcNow
            };

            try
            {
                await _userService.AddUserAsync(userEntity);
            }
            catch (RequestFailedException e) when (e.Status == StatusCodes.Status409Conflict)
            {
                return null;
            }

            _logger.LogInformation("Registered {role} user {email}.", role, email);

            return userEntity;
        }

        public async Task<AuthResponseDto?> LoginAsync(LoginRequestDto credentials)
        {
            UserEntity? user = await _userService.GetUserByEmailAsync(credentials.Email);

            if (user == null)
            {
                VerifyPassword(credentials.Password, _dummyPasswordHash);  // OWASP Foundation, 2026a

                _logger.LogWarning("Login failed for unknown user {email}.", credentials.Email);

                return null;
            }

            if (!VerifyPassword(credentials.Password, user.PasswordHash))
            {
                _logger.LogWarning("Login failed for {email}: wrong password.", user.RowKey);

                return null;
            }

            _logger.LogInformation("{role} user {email} logged in.", user.PartitionKey, user.RowKey);

            return _tokenService.GenerateToken(user);
        }

        public async Task EnsureSeedAdminAsync(SeedAdminSettings seedAdmin)
        {
            if (string.IsNullOrWhiteSpace(seedAdmin.Email) || string.IsNullOrWhiteSpace(seedAdmin.Password))
            {
                _logger.LogWarning("No seed admin is configured, so only Customer accounts can be registered.");

                return;
            }

            if (await _userService.GetUserByEmailAsync(seedAdmin.Email) != null)
            {
                return;
            }

            await RegisterAsync(new RegisterRequestDto
            {
                Email = seedAdmin.Email,
                Password = seedAdmin.Password,
                FullName = seedAdmin.FullName,
                Role = Roles.Admin
            });

            _logger.LogInformation("Seed admin {email} was created.", seedAdmin.Email);
        }
    }
}
