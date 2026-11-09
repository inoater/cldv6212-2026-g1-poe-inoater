namespace CoffeeNChillGatewayAPI.Models
{
    public class FunctionStaffDocument
    {
        public string FileName { get; set; } = string.Empty;

        public long Size { get; set; }

        public DateTimeOffset? LastModified { get; set; }
    }
}
