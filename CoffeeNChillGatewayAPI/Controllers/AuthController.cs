using CoffeeNChillGatewayAPI.Constants;
using CoffeeNChillGatewayAPI.DTOs;
using CoffeeNChillGatewayAPI.Models;
using CoffeeNChillGatewayAPI.Services;
using CoffeeNChillGatewayAPI.Swagger;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace CoffeeNChillGatewayAPI.Controllers
{
    /// <summary>
    /// Registers users in the Users table and issues signed JWT bearer tokens.
    /// </summary>
    [Route("api/auth")]
    [EnableRateLimiting(AuthController.RateLimitPolicy)]    // Microsoft, 2026al
    public class AuthController : GatewayControllerBase     // rudderz243, 2026; Microsoft, 2026w
    {
        public const string RateLimitPolicy = "auth";

        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Creates a user account with a BCrypt-hashed password.
        /// </summary>
        /// <remarks>
        /// Anyone can register a Customer account. Only a logged-in Admin can create Admin or Staff
        /// accounts, so send an Admin bearer token when Role is Admin or Staff.
        /// </remarks>
        /// <param name="newUser">Email, password, full name and role of the new user.</param>
        /// <response code="201">The user was created.</response>
        /// <response code="400">The request body failed validation.</response>
        /// <response code="401">Role is Admin or Staff and no bearer token was sent.</response>
        /// <response code="403">Role is Admin or Staff and the caller is not an Admin.</response>
        /// <response code="409">A user with that email already exists.</response>
        [HttpPost("register")]
        [AllowAnonymous]
        [AcceptsBearerToken]
        [ProducesResponseType(typeof(RegisterResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto newUser)
        {
            bool isPrivilegedRole =
                !string.Equals(newUser.Role, Roles.Customer, StringComparison.OrdinalIgnoreCase);

            if (isPrivilegedRole && User.Identity?.IsAuthenticated != true)
            {
                return ErrorResult(
                    StatusCodes.Status401Unauthorized,
                    "Log in as an Admin to create Admin or Staff accounts.");
            }

            if (isPrivilegedRole && !User.IsInRole(Roles.Admin))   // Microsoft, 2026x
            {
                return ErrorResult(
                    StatusCodes.Status403Forbidden,
                    "Only an Admin can create Admin or Staff accounts.");
            }

            UserEntity? createdUser = await _authService.RegisterAsync(newUser);

            if (createdUser == null)
            {
                return ErrorResult(
                    StatusCodes.Status409Conflict,
                    $"A user with the email '{newUser.Email.Trim().ToLowerInvariant()}' already exists.");
            }

            var response = new RegisterResponseDto
            {
                Email = createdUser.RowKey,
                FullName = createdUser.FullName,
                Role = createdUser.PartitionKey,
                CreatedDate = createdUser.CreatedDate
            };

            return StatusCode(StatusCodes.Status201Created, response);
        }

        /// <summary>
        /// Checks the credentials against the Users table and returns a signed JWT bearer token.
        /// </summary>
        /// <remarks>
        /// The token contains the Name, Email and Role claims. Copy the Token value into the
        /// Swagger Authorize button (or a Postman Bearer token) to call protected endpoints.
        /// </remarks>
        /// <param name="credentials">Email and password.</param>
        /// <response code="200">Login succeeded and a token was issued.</response>
        /// <response code="400">The request body failed validation.</response>
        /// <response code="401">The email or password is wrong.</response>
        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto credentials)
        {
            AuthResponseDto? authResult = await _authService.LoginAsync(credentials);

            if (authResult == null)
            {
                return ErrorResult(StatusCodes.Status401Unauthorized, "Invalid email or password.");
            }

            return Ok(authResult);
        }

        /// <summary>
        /// Returns the user described by the bearer token.
        /// </summary>
        /// <response code="200">The token is valid.</response>
        [HttpGet("me")]
        [Authorize]
        [ProducesResponseType(typeof(CurrentUserDto), StatusCodes.Status200OK)]
        public IActionResult Me()
        {
            string? expiryClaim = User.FindFirstValue("exp");

            DateTime? expiresAtUtc = long.TryParse(expiryClaim, out long expirySeconds)
                ? DateTimeOffset.FromUnixTimeSeconds(expirySeconds).UtcDateTime
                : null;

            return Ok(new CurrentUserDto
            {
                Email = User.FindFirstValue(ClaimTypes.Email) ?? string.Empty,
                FullName = User.FindFirstValue(ClaimTypes.Name) ?? string.Empty,
                Role = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty,
                ExpiresAtUtc = expiresAtUtc
            });
        }
    }
}
