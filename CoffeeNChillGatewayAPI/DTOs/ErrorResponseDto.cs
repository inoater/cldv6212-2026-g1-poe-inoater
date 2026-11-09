namespace CoffeeNChillGatewayAPI.DTOs
{
    public record ErrorResponseDto  // rudderz243, 2026; W3Schools, 2026e
    {
        public int StatusCode { get; init; }

        public string Message { get; init; } = string.Empty;

        public string? Detailed { get; init; }

        public DateTime TimestampUtc { get; init; } = DateTime.UtcNow;
    }
}
