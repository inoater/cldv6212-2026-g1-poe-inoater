using System.ComponentModel.DataAnnotations;

namespace CoffeeNChillGatewayAPI.DTOs
{
    public record MenuItemCreateRequestDto  // Microsoft, 2026ab; rudderz243, 2026; W3Schools, 2026c
    {
        [Required]
        public string PartitionKey { get; init; } = string.Empty;

        [Required]
        public string RowKey { get; init; } = string.Empty;

        [Required]
        public string Name { get; init; } = string.Empty;

        public string Description { get; init; } = string.Empty;

        [Range(0, 10000)]   // Microsoft, 2026ac
        public double Price { get; init; }

        public bool IsAvailable { get; init; } = true;
    }

    public record MenuItemUpdateRequestDto
    {
        [Required]
        public string Name { get; init; } = string.Empty;

        public string Description { get; init; } = string.Empty;

        [Range(0, 10000)]
        public double Price { get; init; }

        public bool IsAvailable { get; init; }
    }

    public record MenuItemResponseDto
    {
        public string PartitionKey { get; init; } = string.Empty;

        public string RowKey { get; init; } = string.Empty;

        public string Name { get; init; } = string.Empty;

        public string Description { get; init; } = string.Empty;

        public double Price { get; init; }

        public bool IsAvailable { get; init; }

        public DateTimeOffset? Timestamp { get; init; }
    }
}
