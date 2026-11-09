using CoffeeNChillGatewayAPI.Middleware;
using Microsoft.AspNetCore.Mvc;

namespace CoffeeNChillGatewayAPI.Controllers
{
    [ApiController]
    [Produces("application/json")]
    public abstract class GatewayControllerBase : ControllerBase    // Microsoft, 2026u; W3Schools, 2026f
    {
        protected ObjectResult ErrorResult(int statusCode, string message)
        {
            return StatusCode(statusCode, ErrorResponseWriter.Create(HttpContext, statusCode, message));
        }
    }
}
