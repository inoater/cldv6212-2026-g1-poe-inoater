using System.ComponentModel.DataAnnotations;

namespace CoffeeNChillGatewayAPI.DTOs
{
    public record OrderSubmissionRequestDto     // Microsoft, 2026ab; rudderz243, 2026
    {
        public string OrderId { get; init; } = string.Empty;

        [Required]
        public string CustomerName { get; init; } = string.Empty;

        [Required, MinLength(1)]
        public List<string> SelectedItemSKUs { get; init; } = new List<string>();

        [Range(0.01, 100000)]
        public double TotalPrice { get; init; }

        public DateTime OrderTimestamp { get; init; }
    }

    public record OrderSubmissionResponseDto
    {
        public string OrderId { get; init; } = string.Empty;

        public string Message { get; init; } = string.Empty;

        public DateTime QueuedAtUtc { get; init; }
    }

    public record OrderStatusUpdateRequestDto
    {
        [Required]
        public string OrderId { get; init; } = string.Empty;

        [Required]
        [RegularExpression("^(Received|Preparing|Ready|Collected)$", ErrorMessage = "Status must be Received, Preparing, Ready or Collected")]
        public string Status { get; init; } = string.Empty;
    }

    public record OrderResponseDto
    {
        public string PartitionKey { get; init; } = string.Empty;

        public string RowKey { get; init; } = string.Empty;

        public string CustomerName { get; init; } = string.Empty;

        public string SelectedItemSKUs { get; init; } = string.Empty;

        public double TotalPrice { get; init; }

        public DateTimeOffset OrderTimestamp { get; init; }

        public string Status { get; init; } = string.Empty;

        public DateTimeOffset StatusUpdatedAt { get; init; }
    }
}
