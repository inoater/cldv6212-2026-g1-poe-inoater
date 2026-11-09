namespace CoffeeNChillGatewayAPI.DTOs
{
    /// <summary>
    /// The body returned for every error response.
    /// </summary>
    public record ErrorResponseDto  // rudderz243, 2026; Microsoft, 2026z; W3Schools, 2026e
    {
        /// <summary>HTTP status code (4xx is a client error, 5xx is a server error).</summary>
        public int StatusCode { get; init; }

        /// <summary>What went wrong.</summary>
        public string Message { get; init; } = string.Empty;

        /// <summary>Extra detail, only included when running in Development.</summary>
        public string? Detailed { get; init; }

        /// <summary>Validation errors per field, when the request body was invalid.</summary>
        public IDictionary<string, string[]>? Errors { get; init; }

        /// <summary>Trace ID to match the error with the gateway logs.</summary>
        public string? TraceId { get; init; }

        /// <summary>When the error happened (UTC).</summary>
        public DateTime TimestampUtc { get; init; } = DateTime.UtcNow;
    }

    /// <summary>
    /// A simple confirmation message.
    /// </summary>
    public record MessageResponseDto
    {
        /// <summary>Confirmation message.</summary>
        public string Message { get; init; } = string.Empty;
    }
}
