using CoffeeNChillGatewayAPI.DTOs;
using CoffeeNChillGatewayAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoffeeNChillGatewayAPI.Controllers
{
    [ApiController]
    [Route("api/auth")]
    [Produces("application/json")]
    public class AuthController : ControllerBase    // Microsoft, 2026u; rudderz243, 2026; W3Schools, 2026f
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegistrationDto dto)
        {
            bool success = await _authService.RegisterAsync(dto);

            if (!success)
            {
                return BadRequest(new ErrorResponseDto
                {
                    StatusCode = 400,
                    Message = $"User with email '{dto.Email}' already exists."
                });
            }

            return StatusCode(StatusCodes.Status201Created, new { message = "User registered successfully." });
        }

        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var authResult = await _authService.LoginAsync(dto);

            if (authResult == null)
            {
                return Unauthorized(new ErrorResponseDto
                {
                    StatusCode = 401,
                    Message = "Invalid credentials provided."
                });
            }

            return Ok(authResult);
        }
    }
}
