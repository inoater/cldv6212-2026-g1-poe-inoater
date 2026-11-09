namespace CoffeeNChillGatewayAPI.Models
{
    public class FunctionOrder
    {
        public string PartitionKey { get; set; } = string.Empty;

        public string RowKey { get; set; } = string.Empty;

        public string CustomerName { get; set; } = string.Empty;

        public string SelectedItemSKUs { get; set; } = string.Empty;

        public double TotalPrice { get; set; }

        public DateTimeOffset OrderTimestamp { get; set; }

        public string Status { get; set; } = string.Empty;

        public DateTimeOffset StatusUpdatedAt { get; set; }
    }
}
