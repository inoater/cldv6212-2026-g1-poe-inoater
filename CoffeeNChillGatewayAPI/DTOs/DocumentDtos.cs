namespace CoffeeNChillGatewayAPI.DTOs
{
    public record DocumentMetadataDto   // rudderz243, 2026
    {
        public string FileName { get; init; } = string.Empty;

        public long Size { get; init; }

        public DateTimeOffset? LastModified { get; init; }
    }
}
