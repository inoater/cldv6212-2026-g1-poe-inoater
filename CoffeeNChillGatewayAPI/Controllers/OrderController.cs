using CoffeeNChillGatewayAPI.Clients;
using CoffeeNChillGatewayAPI.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoffeeNChillGatewayAPI.Controllers
{
    [ApiController]
    [Route("api/orders")]
    [Produces("application/json")]
    public class OrderController : ControllerBase   // Microsoft, 2026ah; Microsoft, 2026s; rudderz243, 2026
    {
        private readonly OrderFunctionClient _orderClient;

        public OrderController(OrderFunctionClient orderClient)
        {
            _orderClient = orderClient;
        }

        [HttpPost("queue")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(OrderSubmissionResponseDto), StatusCodes.Status202Accepted)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Enqueue([FromBody] OrderSubmissionRequestDto dto)
        {
            var result = await _orderClient.QueueOrderAsync(dto);

            return Accepted(new OrderSubmissionResponseDto
            {
                OrderId = result?.OrderId ?? string.Empty,
                Message = "Order placed on the queue.",
                QueuedAtUtc = DateTime.UtcNow
            });
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Staff")]  // Microsoft, 2026x
        [ProducesResponseType(typeof(IEnumerable<OrderResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetAll()
        {
            var result = await _orderClient.GetAllOrdersAsync();
            return Ok(result);
        }

        [HttpGet("{orderId}")]
        [Authorize(Roles = "Admin,Staff")]
        [ProducesResponseType(typeof(OrderResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById([FromRoute] string orderId)
        {
            var result = await _orderClient.GetOrderByIdAsync(orderId);
            return Ok(result);
        }

        [HttpPut("status")]
        [Authorize(Roles = "Admin,Staff")]
        [ProducesResponseType(typeof(OrderResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateStatus([FromBody] OrderStatusUpdateRequestDto dto)
        {
            var result = await _orderClient.UpdateOrderStatusAsync(dto);
            return Ok(result);
        }
    }
}
