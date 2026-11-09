namespace CoffeeNChill.Models
{
    public class OrderStatusUpdate
    {
        public static readonly string[] ValidStatuses =
        {
            "Received",
            "Preparing",
            "Ready",
            "Collected"
        };

        public string OrderId { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;
    }
}
