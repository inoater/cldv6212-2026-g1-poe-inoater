using System.ComponentModel.DataAnnotations;

namespace CoffeeNChillGatewayAPI.Configuration
{
    public class JwtSettings    // Microsoft, 2026ab
    {
        public const string SectionName = "JwtSettings";

        [Required]
        [MinLength(32, ErrorMessage = "The JWT secret key must be at least 32 characters long.")]
        public string SecretKey { get; set; } = string.Empty;

        [Required]
        public string Issuer { get; set; } = string.Empty;

        [Required]
        public string Audience { get; set; } = string.Empty;

        [Range(1, 1440)]
        public int ExpirationMinutes { get; set; } = 60;
    }
}
