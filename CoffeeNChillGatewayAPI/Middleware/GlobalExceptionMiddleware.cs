using CoffeeNChillGatewayAPI.DTOs;
using System.Net;
using System.Text.Json;

namespace CoffeeNChillGatewayAPI.Middleware
{
    public class GlobalExceptionMiddleware  // Microsoft, 2026y; Microsoft, 2026z; rudderz243, 2026
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                await WriteErrorResponse(context, HttpStatusCode.NotFound, "The requested item was not found", ex.Message);
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Conflict)
            {
                await WriteErrorResponse(context, HttpStatusCode.Conflict, "The item already exists", ex.Message);
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.BadRequest ||
                                                  ex.StatusCode == HttpStatusCode.UnsupportedMediaType)
            {
                await WriteErrorResponse(context, ex.StatusCode.Value, "The Function rejected the request", ex.Message);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Failed to communicate with the Functions app");
                await WriteErrorResponse(context, HttpStatusCode.BadGateway, "Can't connect to Function", ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "General unhandled exception occurred");
                await WriteErrorResponse(context, HttpStatusCode.InternalServerError, "General error occurred", ex.Message);
            }   // W3Schools, 2026d
        }

        private static async Task WriteErrorResponse(HttpContext context, HttpStatusCode code, string message, string details)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)code;

            var error = new ErrorResponseDto
            {
                StatusCode = (int)code,
                Message = message,
                Detailed = details
            };

            var json = JsonSerializer.Serialize(error);
            await context.Response.WriteAsync(json);
        }
    }
}
