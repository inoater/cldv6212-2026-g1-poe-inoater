namespace CoffeeNChillGatewayAPI.Models
{
    public class FunctionMenuItem
    {
        public string PartitionKey { get; set; } = string.Empty;

        public string RowKey { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public double Price { get; set; }

        public bool IsAvailable { get; set; }

        public DateTimeOffset? Timestamp { get; set; }
    }
}
