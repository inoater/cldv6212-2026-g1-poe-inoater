namespace CoffeeNChillGatewayAPI.Models
{
    public class FunctionOrderMessage
    {
        public string? OrderId { get; set; }

        public string CustomerName { get; set; } = string.Empty;

        public List<string> SelectedItemSKUs { get; set; } = new List<string>();

        public double TotalPrice { get; set; }

        public DateTime? OrderTimestamp { get; set; }
    }
}
