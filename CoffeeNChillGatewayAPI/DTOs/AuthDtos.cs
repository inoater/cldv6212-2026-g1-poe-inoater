using System.ComponentModel.DataAnnotations;

namespace CoffeeNChillGatewayAPI.DTOs
{
    /// <summary>
    /// The details needed to create a user account.
    /// </summary>
    public record RegisterRequestDto    // rudderz243, 2026
    {
        /// <summary>Email address, used as the username and stored as the RowKey.</summary>
        [Required]
        [EmailAddress]
        [StringLength(100)]
        [RegularExpression(@"^[^/\\#?\s]+@[^/\\#?\s]+\.[^/\\#?\s]+$", ErrorMessage = "Please enter a valid email address.")]
        public string Email { get; init; } = string.Empty;

        /// <summary>At least 8 characters with an uppercase letter, a lowercase letter and a number.</summary>
        [Required]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters long.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$",
            ErrorMessage = "Password must contain an uppercase letter, a lowercase letter and a number.")]    // OWASP Foundation, 2026a
        public string Password { get; init; } = string.Empty;

        /// <summary>Full name of the user.</summary>
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string FullName { get; init; } = string.Empty;

        /// <summary>Admin, Staff or Customer. Only an Admin can create Admin or Staff accounts.</summary>
        [RegularExpression("^(Admin|Staff|Customer)$", ErrorMessage = "Role must be Admin, Staff or Customer.")]
        public string Role { get; init; } = "Customer";
    }

    /// <summary>
    /// The account that was created.
    /// </summary>
    public record RegisterResponseDto
    {
        /// <summary>Email address of the new user.</summary>
        public string Email { get; init; } = string.Empty;

        /// <summary>Full name of the new user.</summary>
        public string FullName { get; init; } = string.Empty;

        /// <summary>Role of the new user.</summary>
        public string Role { get; init; } = string.Empty;

        /// <summary>When the account was created (UTC).</summary>
        public DateTime CreatedDate { get; init; }
    }

    /// <summary>
    /// Login credentials.
    /// </summary>
    public record LoginRequestDto
    {
        /// <summary>Email address of the user.</summary>
        [Required]
        [EmailAddress]
        public string Email { get; init; } = string.Empty;

        /// <summary>Password of the user.</summary>
        [Required]
        public string Password { get; init; } = string.Empty;
    }

    /// <summary>
    /// A signed JWT bearer token and the user it belongs to.
    /// </summary>
    public record AuthResponseDto
    {
        /// <summary>Signed JWT. Send it as "Authorization: Bearer {token}".</summary>
        public string Token { get; init; } = string.Empty;

        /// <summary>Always "Bearer".</summary>
        public string TokenType { get; init; } = "Bearer";

        /// <summary>When the token stops working (UTC).</summary>
        public DateTime ExpiresAtUtc { get; init; }

        /// <summary>Email address of the user.</summary>
        public string Email { get; init; } = string.Empty;

        /// <summary>Full name of the user.</summary>
        public string FullName { get; init; } = string.Empty;

        /// <summary>Role of the user.</summary>
        public string Role { get; init; } = string.Empty;
    }

    /// <summary>
    /// The user described by the bearer token that was sent.
    /// </summary>
    public record CurrentUserDto
    {
        /// <summary>Email claim from the token.</summary>
        public string Email { get; init; } = string.Empty;

        /// <summary>Name claim from the token.</summary>
        public string FullName { get; init; } = string.Empty;

        /// <summary>Role claim from the token.</summary>
        public string Role { get; init; } = string.Empty;

        /// <summary>When the token stops working (UTC).</summary>
        public DateTime? ExpiresAtUtc { get; init; }
    }
}
