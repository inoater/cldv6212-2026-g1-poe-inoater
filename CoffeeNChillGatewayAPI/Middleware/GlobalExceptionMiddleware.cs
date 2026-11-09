using Azure;
using CoffeeNChillGatewayAPI.Exceptions;
using System.Net;

namespace CoffeeNChillGatewayAPI.Middleware
{
    public class GlobalExceptionMiddleware  // Microsoft, 2026y; Microsoft, 2026z; rudderz243, 2026
    {
        private static readonly HttpStatusCode[] _forwardedClientErrors =
        {
            HttpStatusCode.BadRequest,
            HttpStatusCode.NotFound,
            HttpStatusCode.Conflict,
            HttpStatusCode.UnsupportedMediaType,
            HttpStatusCode.ServiceUnavailable
        };

        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;
        private readonly IHostEnvironment _environment;

        public GlobalExceptionMiddleware(
            RequestDelegate next,
            ILogger<GlobalExceptionMiddleware> logger,
            IHostEnvironment environment)
        {
            _next = next;
            _logger = logger;
            _environment = environment;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (FunctionsApiException e) when (_forwardedClientErrors.Contains(e.StatusCode))
            {
                _logger.LogWarning(
                    "The Functions app returned {statusCode} for {method} {path}: {message}",
                    (int)e.StatusCode,
                    context.Request.Method,
                    context.Request.Path,
                    e.Message);

                await ErrorResponseWriter.WriteAsync(context, (int)e.StatusCode, e.Message);
            }
            catch (FunctionsApiException e)
            {
                _logger.LogError(
                    e,
                    "The Functions app failed with {statusCode} for {method} {path}.",
                    (int)e.StatusCode,
                    context.Request.Method,
                    context.Request.Path);

                await ErrorResponseWriter.WriteAsync(
                    context,
                    StatusCodes.Status502BadGateway,
                    "The CoffeeNChill Functions app could not complete the request.",
                    Detail(e));
            }
            catch (BadHttpRequestException e)
            {
                _logger.LogWarning(e, "Bad request for {path}.", context.Request.Path);

                await ErrorResponseWriter.WriteAsync(
                    context,
                    e.StatusCode,
                    e.StatusCode == StatusCodes.Status413PayloadTooLarge
                        ? "The request body is too large."
                        : "The request could not be read.",
                    Detail(e));
            }
            catch (HttpRequestException e)
            {
                _logger.LogError(e, "Could not reach the Functions app for {path}.", context.Request.Path);

                await ErrorResponseWriter.WriteAsync(
                    context,
                    StatusCodes.Status502BadGateway,
                    "The CoffeeNChill Functions app is unreachable. Please try again shortly.",
                    Detail(e));
            }
            catch (TaskCanceledException e) when (!context.RequestAborted.IsCancellationRequested)
            {
                _logger.LogError(e, "The Functions app timed out for {path}.", context.Request.Path);

                await ErrorResponseWriter.WriteAsync(
                    context,
                    StatusCodes.Status504GatewayTimeout,
                    "The CoffeeNChill Functions app took too long to respond.",
                    Detail(e));
            }
            catch (RequestFailedException e)
            {
                _logger.LogError(e, "Azure Storage request failed for {path}.", context.Request.Path);

                await ErrorResponseWriter.WriteAsync(
                    context,
                    StatusCodes.Status503ServiceUnavailable,
                    "Storage is currently unavailable. Please try again shortly.",
                    Detail(e));
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unhandled exception for {method} {path}.", context.Request.Method, context.Request.Path);

                await ErrorResponseWriter.WriteAsync(
                    context,
                    StatusCodes.Status500InternalServerError,
                    "An unexpected error occurred.",
                    Detail(e));     // W3Schools, 2026d
            }
        }

        private string? Detail(Exception e)
        {
            return _environment.IsDevelopment() ? e.Message : null;     // OWASP Foundation, 2026b
        }
    }
}
