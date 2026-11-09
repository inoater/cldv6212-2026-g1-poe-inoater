using System.Net;

namespace CoffeeNChillGatewayAPI.Exceptions
{
    public class FunctionsApiException : Exception  // W3Schools, 2026f
    {
        public FunctionsApiException(HttpStatusCode statusCode, string message)
            : base(message)
        {
            StatusCode = statusCode;
        }

        public HttpStatusCode StatusCode { get; }
    }
}
