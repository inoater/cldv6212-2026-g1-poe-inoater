using System.ComponentModel.DataAnnotations;

namespace CoffeeNChillGatewayAPI.DTOs
{
    /// <summary>
    /// A menu item as it is returned to client applications.
    /// </summary>
    public record MenuItemDto   // Microsoft, 2026ac; rudderz243, 2026; W3Schools, 2026c
    {
        /// <summary>Menu category, stored as the PartitionKey (e.g. "Hot Drinks").</summary>
        public string Category { get; init; } = string.Empty;

        /// <summary>Item SKU, stored as the RowKey (e.g. "COF-001").</summary>
        public string Id { get; init; } = string.Empty;

        /// <summary>Item name shown on the menu.</summary>
        public string Name { get; init; } = string.Empty;

        /// <summary>Short menu description.</summary>
        public string Description { get; init; } = string.Empty;

        /// <summary>Price in Rand.</summary>
        public double Price { get; init; }

        /// <summary>Whether the item can currently be ordered.</summary>
        public bool IsAvailable { get; init; }

        /// <summary>When the item was last changed in the MenuItems table.</summary>
        public DateTimeOffset? LastUpdated { get; init; }
    }

    /// <summary>
    /// The details needed to add a new item to the menu.
    /// </summary>
    public record CreateMenuItemDto
    {
        /// <summary>Menu category (e.g. "Hot Drinks", "Cold Drinks", "Pastries", "Sandwiches").</summary>
        [Required]
        [StringLength(50, MinimumLength = 2)]
        [RegularExpression(@"^[^/\\#?]+$", ErrorMessage = "Category cannot contain the characters / \\ # ?")]   // Microsoft, 2026ad
        public string Category { get; init; } = string.Empty;

        /// <summary>Unique item SKU (e.g. "COF-001").</summary>
        [Required]
        [RegularExpression(@"^[A-Za-z0-9-]{3,20}$", ErrorMessage = "Id must be 3 to 20 letters, numbers or dashes.")]
        public string Id { get; init; } = string.Empty;

        /// <summary>Item name shown on the menu.</summary>
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string Name { get; init; } = string.Empty;

        /// <summary>Short menu description.</summary>
        [StringLength(500)]
        public string Description { get; init; } = string.Empty;

        /// <summary>Price in Rand. Cannot be negative.</summary>
        [Range(0.0, 10000.0, ErrorMessage = "Price must be between 0 and 10000.")]
        public double Price { get; init; }

        /// <summary>Whether the item can be ordered straight away.</summary>
        public bool IsAvailable { get; init; } = true;
    }

    /// <summary>
    /// Changes to an existing menu item. Only the fields that are sent are changed.
    /// </summary>
    public record UpdateMenuItemDto : IValidatableObject
    {
        /// <summary>New item name.</summary>
        [StringLength(100, MinimumLength = 2)]
        public string? Name { get; init; }

        /// <summary>New menu description.</summary>
        [StringLength(500)]
        public string? Description { get; init; }

        /// <summary>New price in Rand.</summary>
        [Range(0.0, 10000.0, ErrorMessage = "Price must be between 0 and 10000.")]
        public double? Price { get; init; }

        /// <summary>New availability status.</summary>
        public bool? IsAvailable { get; init; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)  // Microsoft, 2026ad
        {
            if (Name == null && Description == null && Price == null && IsAvailable == null)
            {
                yield return new ValidationResult(
                    "Send at least one of Name, Description, Price or IsAvailable.",
                    new[] { nameof(Price), nameof(IsAvailable) });
            }
        }
    }
}
