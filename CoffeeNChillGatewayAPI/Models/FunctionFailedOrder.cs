namespace CoffeeNChillGatewayAPI.Models
{
    public class FunctionFailedOrder
    {
        public string PartitionKey { get; set; } = string.Empty;

        public string RowKey { get; set; } = string.Empty;

        public string Payload { get; set; } = string.Empty;

        public string FailureReason { get; set; } = string.Empty;

        public long DequeueCount { get; set; }

        public DateTimeOffset? EnqueuedTime { get; set; }
    }
}
