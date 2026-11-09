using CoffeeNChillGatewayAPI.DTOs;
using System.Diagnostics;
using System.Text.Json;

namespace CoffeeNChillGatewayAPI.Middleware
{
    public static class ErrorResponseWriter     // Microsoft, 2026z
    {
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = null
        };

        public static ErrorResponseDto Create(
            HttpContext context,
            int statusCode,
            string message,
            string? detailed = null,
            IDictionary<string, string[]>? errors = null)
        {
            return new ErrorResponseDto
            {
                StatusCode = statusCode,
                Message = message,
                Detailed = detailed,
                Errors = errors,
                TraceId = Activity.Current?.Id ?? context.TraceIdentifier,
                TimestampUtc = DateTime.UtcNow
            };
        }

        public static async Task WriteAsync(
            HttpContext context,
            int statusCode,
            string message,
            string? detailed = null)
        {
            if (context.Response.HasStarted)
            {
                return;
            }

            context.Response.Clear();
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";

            ErrorResponseDto error = Create(context, statusCode, message, detailed);

            await context.Response.WriteAsync(JsonSerializer.Serialize(error, _jsonOptions));
        }
    }
}
