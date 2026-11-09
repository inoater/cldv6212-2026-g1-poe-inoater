using Azure;
using CoffeeNChill.Models;
using CoffeeNChill.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace CoffeeNChill.Functions
{
    public class UpdateOrderStatus
    {
        private readonly ILogger<UpdateOrderStatus> _logger;
        private readonly OrderService _orderService;

        public UpdateOrderStatus(
            ILogger<UpdateOrderStatus> logger,
            OrderService orderService)
        {
            _logger = logger;
            _orderService = orderService;
        }

        [Function("UpdateOrderStatus")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "orders/status")]   // Microsoft, 2026c
            HttpRequestData req)
        {
            _logger.LogInformation("Updating an order status.");

            try
            {
                var statusUpdate =
                    await JsonSerializer.DeserializeAsync<OrderStatusUpdate>(   // W3Schools, 2026b
                        req.Body,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (statusUpdate == null ||
                    string.IsNullOrWhiteSpace(statusUpdate.OrderId) ||
                    string.IsNullOrWhiteSpace(statusUpdate.Status))
                {
                    var errorResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                    await errorResponse.WriteStringAsync("Order ID and Status are required.");
                    return errorResponse;
                }

                string? newStatus = OrderStatusUpdate.ValidStatuses.FirstOrDefault(
                    status => string.Equals(status, statusUpdate.Status.Trim(), StringComparison.OrdinalIgnoreCase));

                if (newStatus == null)
                {
                    var errorResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                    await errorResponse.WriteStringAsync(
                        "Status must be Received, Preparing, Ready or Collected.");
                    return errorResponse;
                }

                OrderEntity? order =
                    await _orderService.GetOrderByIdAsync(statusUpdate.OrderId.Trim());

                if (order == null)
                {
                    var errorResponse = req.CreateResponse(HttpStatusCode.NotFound);
                    await errorResponse.WriteStringAsync("Order was not found.");
                    return errorResponse;
                }

                string previousStatus = order.Status;

                order.Status = newStatus;
                order.StatusUpdatedAt = DateTimeOffset.UtcNow;

                await _orderService.UpdateOrderAsync(order);

                _logger.LogInformation(
                    "Order {orderId} was manually moved from {previousStatus} to {newStatus}.",
                    order.RowKey,
                    previousStatus,
                    order.Status);

                var response = req.CreateResponse(HttpStatusCode.OK);

                await response.WriteAsJsonAsync(order);

                return response;
            }
            catch (JsonException)
            {
                var response = req.CreateResponse(HttpStatusCode.BadRequest);

                await response.WriteStringAsync("The status update must be sent as valid JSON.");

                return response;
            }
            catch (RequestFailedException e) when (e.Status == (int)HttpStatusCode.PreconditionFailed)
            {
                _logger.LogWarning(e, "The order changed while its status was being updated.");

                var response = req.CreateResponse(HttpStatusCode.Conflict);

                await response.WriteStringAsync(
                    "The order was updated by another process. Please try again.");

                return response;    // Microsoft, 2026j
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error updating order status.");

                var response = req.CreateResponse(
                    HttpStatusCode.InternalServerError);

                await response.WriteStringAsync(
                    "An error occurred while updating the order status.");

                return response;    // IIE, 2026d
            }
        }
    }
}
