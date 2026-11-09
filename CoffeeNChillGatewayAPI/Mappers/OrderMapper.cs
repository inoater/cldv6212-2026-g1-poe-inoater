using CoffeeNChillGatewayAPI.DTOs;
using CoffeeNChillGatewayAPI.Models;

namespace CoffeeNChillGatewayAPI.Mappers
{
    public static class OrderMapper     // Microsoft, 2026ac
    {
        public static FunctionOrderMessage ToFunctionMessage(this PlaceOrderDto newOrder)
        {
            return new FunctionOrderMessage
            {
                OrderId = string.IsNullOrWhiteSpace(newOrder.OrderId) ? null : newOrder.OrderId.Trim(),
                CustomerName = newOrder.CustomerName.Trim(),
                SelectedItemSKUs = newOrder.SelectedItemSKUs.Select(sku => sku.Trim()).ToList(),
                TotalPrice = newOrder.TotalPrice,
                OrderTimestamp = newOrder.OrderTimestamp
            };
        }

        public static QueuedOrderDto ToQueuedDto(this FunctionOrderMessage queuedOrder)
        {
            return new QueuedOrderDto
            {
                OrderId = queuedOrder.OrderId ?? string.Empty,
                CustomerName = queuedOrder.CustomerName,
                SelectedItemSKUs = queuedOrder.SelectedItemSKUs,
                TotalPrice = queuedOrder.TotalPrice,
                OrderTimestamp = queuedOrder.OrderTimestamp ?? DateTime.UtcNow,
                Message = "Your order has been placed on the queue. Use the OrderId to track it."
            };
        }

        public static OrderDto ToDto(this FunctionOrder order)
        {
            return new OrderDto
            {
                OrderId = order.RowKey,
                OrderDate = order.PartitionKey,
                CustomerName = order.CustomerName,
                SelectedItemSKUs = order.SelectedItemSKUs
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .ToList(),
                TotalPrice = order.TotalPrice,
                OrderTimestamp = order.OrderTimestamp,
                Status = order.Status,
                StatusUpdatedAt = order.StatusUpdatedAt
            };
        }

        public static FailedOrderDto ToDto(this FunctionFailedOrder failedOrder)
        {
            return new FailedOrderDto
            {
                Id = failedOrder.RowKey,
                Payload = failedOrder.Payload,
                FailureReason = failedOrder.FailureReason,
                DequeueCount = failedOrder.DequeueCount,
                EnqueuedTime = failedOrder.EnqueuedTime
            };
        }
    }
}
