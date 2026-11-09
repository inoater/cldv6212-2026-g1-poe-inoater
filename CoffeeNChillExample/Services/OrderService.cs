using Azure.Data.Tables;
using CoffeeNChill.Models;

namespace CoffeeNChill.Services
{
    public class OrderService   // Microsoft, 2026d; Microsoft, 2026g
    {
        private readonly TableClient _tableClient;

        private OrderService(TableClient tableClient)
        {
            _tableClient = tableClient;
        }

        public static async Task<OrderService> CreateAsync(string connectionString, string tableName)
        {
            var tableClient = new TableClient(connectionString, tableName);

            await tableClient.CreateIfNotExistsAsync();

            return new OrderService(tableClient);
        }

        public async Task AddOrderAsync(OrderEntity newOrder)
        {
            await _tableClient.AddEntityAsync(newOrder);
        }

        public async Task<List<OrderEntity>> GetAllOrdersAsync()    // W3Schools, 2026b
        {
            var allOrders = new List<OrderEntity>();

            await foreach (OrderEntity order in _tableClient.QueryAsync<OrderEntity>())
            {
                allOrders.Add(order);
            }

            return allOrders;
        }

        public async Task<OrderEntity?> GetOrderByIdAsync(string orderId)
        {
            await foreach (OrderEntity order in _tableClient.QueryAsync<OrderEntity>(
                o => o.RowKey == orderId))
            {
                return order;
            }

            return null;
        }

        public async Task<List<OrderEntity>> GetActiveOrdersAsync()
        {
            var activeOrders = new List<OrderEntity>();

            await foreach (OrderEntity order in _tableClient.QueryAsync<OrderEntity>(
                o => o.Status != "Collected"))
            {
                activeOrders.Add(order);
            }

            return activeOrders;
        }

        public async Task UpdateOrderAsync(OrderEntity order)
        {
            await _tableClient.UpdateEntityAsync(   // Microsoft, 2026j
                order,
                order.ETag,
                TableUpdateMode.Replace);
        }
    }
}