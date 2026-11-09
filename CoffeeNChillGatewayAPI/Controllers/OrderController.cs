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

        /// <summary>
        /// Places an order on the order-processing-queue.
        /// </summary>
        /// <param name="dto">Customer name, item SKUs and total price. OrderId and OrderTimestamp are generated when left out.</param>
        /// <response code="202">The order was queued.</response>
        /// <response code="400">The order details are invalid.</response>
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

        /// <summary>
        /// Gets every order in the Orders table.
        /// </summary>
        /// <response code="200">The orders.</response>
        /// <response code="401">No valid token was sent.</response>
        /// <response code="403">Only Admin and Staff can view all orders.</response>
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

        /// <summary>
        /// Gets one order and its current status.
        /// </summary>
        /// <param name="orderId">Order ID, e.g. ORD-2026-8801.</param>
        /// <response code="200">The order.</response>
        /// <response code="401">No valid token was sent.</response>
        /// <response code="403">Only Admin and Staff can view orders.</response>
        /// <response code="404">The order was not found.</response>
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

        /// <summary>
        /// Updates the status of an order.
        /// </summary>
        /// <param name="dto">Order ID and the new status (Received, Preparing, Ready or Collected).</param>
        /// <response code="200">The status was updated.</response>
        /// <response code="400">The status is invalid.</response>
        /// <response code="401">No valid token was sent.</response>
        /// <response code="403">Only Admin and Staff can update order statuses.</response>
        /// <response code="404">The order was not found.</response>
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
