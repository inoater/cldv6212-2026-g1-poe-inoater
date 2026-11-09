using CoffeeNChillGatewayAPI.Clients;
using CoffeeNChillGatewayAPI.Constants;
using CoffeeNChillGatewayAPI.DTOs;
using CoffeeNChillGatewayAPI.Mappers;
using CoffeeNChillGatewayAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoffeeNChillGatewayAPI.Controllers
{
    /// <summary>
    /// Orders, routed to the order-processing-queue producer and the Orders table functions.
    /// </summary>
    [Route("api/orders")]
    public class OrdersController : GatewayControllerBase   // Microsoft, 2026ah; Microsoft, 2026s; rudderz243, 2026
    {
        private readonly OrderFunctionClient _orderClient;

        public OrdersController(OrderFunctionClient orderClient)
        {
            _orderClient = orderClient;
        }

        /// <summary>
        /// Places an order on the order-processing-queue. No login required.
        /// </summary>
        /// <remarks>
        /// The order is processed in the background, so the response is 202 Accepted straight away.
        /// Use GET /api/orders/{orderId} to track it.
        /// </remarks>
        /// <param name="newOrder">Customer name, item SKUs and total price.</param>
        /// <response code="202">The order was queued.</response>
        /// <response code="400">The request body failed validation.</response>
        /// <response code="503">The order queue is unavailable.</response>
        [HttpPost("queue")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(QueuedOrderDto), StatusCodes.Status202Accepted)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status503ServiceUnavailable)]
        public async Task<IActionResult> PlaceOrder([FromBody] PlaceOrderDto newOrder)
        {
            FunctionOrderMessage queuedOrder = await _orderClient.QueueOrderAsync(newOrder.ToFunctionMessage());

            QueuedOrderDto queuedDto = queuedOrder.ToQueuedDto();

            return Accepted($"/api/orders/{Uri.EscapeDataString(queuedDto.OrderId)}", queuedDto);
        }

        /// <summary>
        /// Returns one order and its current status, so a student can track it. No login required.
        /// </summary>
        /// <param name="orderId">Order ID, e.g. "ORD-2026-8801".</param>
        /// <response code="200">The order.</response>
        /// <response code="404">The order does not exist (or has not been processed yet).</response>
        [HttpGet("{orderId}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status502BadGateway)]
        public async Task<IActionResult> GetById([FromRoute] string orderId)
        {
            FunctionOrder order = await _orderClient.GetOrderByIdAsync(orderId);

            return Ok(order.ToDto());
        }

        /// <summary>
        /// Returns every order in the Orders table, newest first.
        /// </summary>
        /// <response code="200">All orders.</response>
        [HttpGet]
        [Authorize(Roles = Roles.AdminOrStaff)]     // Microsoft, 2026x
        [ProducesResponseType(typeof(IEnumerable<OrderDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status502BadGateway)]
        public async Task<IActionResult> GetAll()
        {
            List<FunctionOrder> orders = await _orderClient.GetAllOrdersAsync();

            return Ok(orders
                .OrderByDescending(order => order.OrderTimestamp)
                .Select(order => order.ToDto()));
        }

        /// <summary>
        /// Manually changes the preparation status of an order.
        /// </summary>
        /// <param name="statusUpdate">Order ID and the new status (Received, Preparing, Ready or Collected).</param>
        /// <response code="200">The status was changed.</response>
        /// <response code="400">The request body failed validation.</response>
        /// <response code="404">The order does not exist.</response>
        /// <response code="409">The order was changed by another process at the same time.</response>
        [HttpPut("status")]
        [Authorize(Roles = Roles.AdminOrStaff)]
        [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status502BadGateway)]
        public async Task<IActionResult> UpdateStatus([FromBody] UpdateOrderStatusDto statusUpdate)
        {
            FunctionOrder updatedOrder = await _orderClient.UpdateOrderStatusAsync(new FunctionStatusUpdate
            {
                OrderId = statusUpdate.OrderId.Trim(),
                Status = statusUpdate.Status
            });

            return Ok(updatedOrder.ToDto());
        }

        /// <summary>
        /// Returns the order messages that failed and were moved to the poison queue.
        /// </summary>
        /// <response code="200">The failed order messages.</response>
        [HttpGet("failed")]
        [Authorize(Roles = Roles.AdminOrStaff)]
        [ProducesResponseType(typeof(IEnumerable<FailedOrderDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status502BadGateway)]
        public async Task<IActionResult> GetFailed()
        {
            List<FunctionFailedOrder> failedOrders = await _orderClient.GetFailedOrdersAsync();

            return Ok(failedOrders.Select(failedOrder => failedOrder.ToDto()));
        }
    }
}
