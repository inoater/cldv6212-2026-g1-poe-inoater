using CoffeeNChillGatewayAPI.Models;

namespace CoffeeNChillGatewayAPI.Clients
{
    public class OrderFunctionClient : FunctionClientBase   // Microsoft, 2026aa; rudderz243, 2026
    {
        public OrderFunctionClient(HttpClient httpClient)
            : base(httpClient)
        {
        }

        public async Task<FunctionOrderMessage> QueueOrderAsync(FunctionOrderMessage newOrder)
        {
            using HttpResponseMessage response =
                await _httpClient.PostAsJsonAsync("api/orders/queue", newOrder, _functionJsonOptions);

            return await ReadFunctionJsonAsync<FunctionOrderMessage>(response);
        }

        public async Task<List<FunctionOrder>> GetAllOrdersAsync()
        {
            using HttpResponseMessage response = await _httpClient.GetAsync("api/orders");

            return await ReadFunctionJsonAsync<List<FunctionOrder>>(response);
        }

        public async Task<FunctionOrder> GetOrderByIdAsync(string orderId)
        {
            using HttpResponseMessage response =
                await _httpClient.GetAsync($"api/orders/{Escape(orderId)}");

            return await ReadFunctionJsonAsync<FunctionOrder>(response);
        }

        public async Task<FunctionOrder> UpdateOrderStatusAsync(FunctionStatusUpdate statusUpdate)
        {
            using HttpResponseMessage response =
                await _httpClient.PutAsJsonAsync("api/orders/status", statusUpdate, _functionJsonOptions);

            return await ReadFunctionJsonAsync<FunctionOrder>(response);
        }

        public async Task<List<FunctionFailedOrder>> GetFailedOrdersAsync()
        {
            using HttpResponseMessage response = await _httpClient.GetAsync("api/failed-orders");

            return await ReadFunctionJsonAsync<List<FunctionFailedOrder>>(response);
        }
    }
}
