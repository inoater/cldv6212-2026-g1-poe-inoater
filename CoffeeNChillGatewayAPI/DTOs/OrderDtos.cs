using System.ComponentModel.DataAnnotations;

namespace CoffeeNChillGatewayAPI.DTOs
{
    /// <summary>
    /// A new order placed by a student.
    /// </summary>
    public record PlaceOrderDto : IValidatableObject    // Microsoft, 2026ac
    {
        /// <summary>Optional order ID (e.g. "ORD-2026-8801"). Generated when left out.</summary>
        [StringLength(50)]
        [RegularExpression(@"^[^/\\#?]+$", ErrorMessage = "Order ID cannot contain the characters / \\ # ?")]
        public string? OrderId { get; init; }

        /// <summary>Name of the student placing the order.</summary>
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string CustomerName { get; init; } = string.Empty;

        /// <summary>SKUs of the menu items in the order.</summary>
        [Required]
        [MinLength(1, ErrorMessage = "At least one item SKU is required.")]
        public List<string> SelectedItemSKUs { get; init; } = new List<string>();

        /// <summary>Total price of the order in Rand.</summary>
        [Range(0.01, 100000.0, ErrorMessage = "Total price must be greater than zero.")]
        public double TotalPrice { get; init; }

        /// <summary>Optional time the order was placed (UTC). Generated when left out.</summary>
        public DateTime? OrderTimestamp { get; init; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)  // Microsoft, 2026ad
        {
            if (SelectedItemSKUs != null && SelectedItemSKUs.Any(sku => string.IsNullOrWhiteSpace(sku)))
            {
                yield return new ValidationResult(
                    "Item SKUs cannot be empty.",
                    new[] { nameof(SelectedItemSKUs) });
            }
        }
    }

    /// <summary>
    /// The order that was placed on the order-processing-queue.
    /// </summary>
    public record QueuedOrderDto
    {
        /// <summary>Order ID used to track the order.</summary>
        public string OrderId { get; init; } = string.Empty;

        /// <summary>Name of the student who placed the order.</summary>
        public string CustomerName { get; init; } = string.Empty;

        /// <summary>SKUs of the menu items in the order.</summary>
        public List<string> SelectedItemSKUs { get; init; } = new List<string>();

        /// <summary>Total price of the order in Rand.</summary>
        public double TotalPrice { get; init; }

        /// <summary>Time the order was placed (UTC).</summary>
        public DateTime OrderTimestamp { get; init; }

        /// <summary>Message for the client.</summary>
        public string Message { get; init; } = string.Empty;
    }

    /// <summary>
    /// An order from the Orders table and its current preparation status.
    /// </summary>
    public record OrderDto
    {
        /// <summary>Order ID, stored as the RowKey.</summary>
        public string OrderId { get; init; } = string.Empty;

        /// <summary>Order date (yyyy-MM-dd), stored as the PartitionKey.</summary>
        public string OrderDate { get; init; } = string.Empty;

        /// <summary>Name of the student who placed the order.</summary>
        public string CustomerName { get; init; } = string.Empty;

        /// <summary>SKUs of the menu items in the order.</summary>
        public List<string> SelectedItemSKUs { get; init; } = new List<string>();

        /// <summary>Total price of the order in Rand.</summary>
        public double TotalPrice { get; init; }

        /// <summary>Time the order was placed (UTC).</summary>
        public DateTimeOffset OrderTimestamp { get; init; }

        /// <summary>Received, Preparing, Ready or Collected.</summary>
        public string Status { get; init; } = string.Empty;

        /// <summary>When the status last changed (UTC).</summary>
        public DateTimeOffset StatusUpdatedAt { get; init; }
    }

    /// <summary>
    /// A manual status change made by a barista or canteen manager.
    /// </summary>
    public record UpdateOrderStatusDto
    {
        /// <summary>Order ID to update.</summary>
        [Required]
        [StringLength(50)]
        [RegularExpression(@"^[^/\\#?]+$", ErrorMessage = "Order ID cannot contain the characters / \\ # ?")]
        public string OrderId { get; init; } = string.Empty;

        /// <summary>New status: Received, Preparing, Ready or Collected.</summary>
        [Required]
        [RegularExpression("^(Received|Preparing|Ready|Collected)$",
            ErrorMessage = "Status must be Received, Preparing, Ready or Collected.")]
        public string Status { get; init; } = string.Empty;
    }

    /// <summary>
    /// A queue message that failed and was moved to the poison queue.
    /// </summary>
    public record FailedOrderDto
    {
        /// <summary>Unique ID of the failed message record.</summary>
        public string Id { get; init; } = string.Empty;

        /// <summary>Raw message that could not be processed.</summary>
        public string Payload { get; init; } = string.Empty;

        /// <summary>Why the message failed.</summary>
        public string FailureReason { get; init; } = string.Empty;

        /// <summary>How many times the message was tried.</summary>
        public long DequeueCount { get; init; }

        /// <summary>When the message was first queued (UTC).</summary>
        public DateTimeOffset? EnqueuedTime { get; init; }
    }
}
