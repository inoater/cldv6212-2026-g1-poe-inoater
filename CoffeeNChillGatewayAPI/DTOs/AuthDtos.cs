using System.ComponentModel.DataAnnotations;

namespace CoffeeNChillGatewayAPI.DTOs
{
    public record RegistrationDto   // rudderz243, 2026; Microsoft, 2026ac
    {
        [Required, EmailAddress]
        public string Email { get; init; } = string.Empty;

        [Required, MinLength(8)]
        public string Password { get; init; } = string.Empty;

        [Required]
        public string FullName { get; init; } = string.Empty;

        [Required]
        [RegularExpression("^(Admin|Staff|Customer)$", ErrorMessage = "Role must be Admin, Staff or Customer")]
        public string Role { get; init; } = "Customer";
    }

    public record LoginDto
    {
        [Required, EmailAddress]
        public string Email { get; init; } = string.Empty;

        [Required]
        public string Password { get; init; } = string.Empty;
    }

    public record AuthResponseDto
    {
        public string Token { get; init; } = string.Empty;

        public string Email { get; init; } = string.Empty;

        public string FullName { get; init; } = string.Empty;

        public string Role { get; init; } = string.Empty;

        public DateTime ExpiresAtUtc { get; init; }
    }
}
