using CoffeeNChillGatewayAPI.DTOs;
using System.Text.Json;

namespace CoffeeNChillGatewayAPI.Clients
{
    public class OrderFunctionClient    // Microsoft, 2026aa; rudderz243, 2026
    {
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions();

        private readonly HttpClient _httpClient;

        public OrderFunctionClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<OrderSubmissionRequestDto?> QueueOrderAsync(OrderSubmissionRequestDto newOrder)
        {
            var response = await _httpClient.PostAsJsonAsync("api/orders/queue", newOrder, _jsonOptions);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<OrderSubmissionRequestDto>();
        }

        public async Task<IEnumerable<OrderResponseDto>> GetAllOrdersAsync()
        {
            var response = await _httpClient.GetAsync("api/orders");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<IEnumerable<OrderResponseDto>>() ?? new List<OrderResponseDto>();
        }

        public async Task<OrderResponseDto?> GetOrderByIdAsync(string orderId)
        {
            var response = await _httpClient.GetAsync($"api/orders/{Uri.EscapeDataString(orderId)}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<OrderResponseDto>();
        }

        public async Task<OrderResponseDto?> UpdateOrderStatusAsync(OrderStatusUpdateRequestDto statusUpdate)
        {
            var response = await _httpClient.PutAsJsonAsync("api/orders/status", statusUpdate, _jsonOptions);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<OrderResponseDto>();
        }
    }
}
