using Azure;
using Azure.Data.Tables;

namespace CoffeeNChillGatewayAPI.Models
{
    public class UserEntity : ITableEntity  // Microsoft, 2026d; rudderz243, 2026
    {
        public string PartitionKey { get; set; } = "Customer";

        public string RowKey { get; set; } = string.Empty;

        public DateTimeOffset? Timestamp { get; set; }

        public ETag ETag { get; set; }

        public string PasswordHash { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}
