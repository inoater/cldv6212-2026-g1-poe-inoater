using CoffeeNChillGatewayAPI.DTOs;
using System.Text.Json;

namespace CoffeeNChillGatewayAPI.Clients
{
    public class MenuFunctionClient     // Microsoft, 2026aa; rudderz243, 2026
    {
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions();

        private readonly HttpClient _httpClient;

        public MenuFunctionClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IEnumerable<MenuItemResponseDto>> GetAllMenuItemsAsync()
        {
            var response = await _httpClient.GetAsync("api/menu");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<IEnumerable<MenuItemResponseDto>>() ?? new List<MenuItemResponseDto>();
        }

        public async Task<IEnumerable<MenuItemResponseDto>> GetMenuItemsByCategoryAsync(string category)
        {
            var response = await _httpClient.GetAsync($"api/menu/category/{Uri.EscapeDataString(category)}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<IEnumerable<MenuItemResponseDto>>() ?? new List<MenuItemResponseDto>();
        }

        public async Task<MenuItemResponseDto?> CreateMenuItemAsync(MenuItemCreateRequestDto newItem)
        {
            var response = await _httpClient.PostAsJsonAsync("api/menu", newItem, _jsonOptions);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<MenuItemResponseDto>();
        }

        public async Task<MenuItemResponseDto?> UpdateMenuItemAsync(string category, string id, MenuItemUpdateRequestDto updatedItem)
        {
            var response = await _httpClient.PutAsJsonAsync(
                $"api/menu/{Uri.EscapeDataString(category)}/{Uri.EscapeDataString(id)}",
                updatedItem,
                _jsonOptions);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<MenuItemResponseDto>();
        }

        public async Task DeleteMenuItemAsync(string category, string id)
        {
            var response = await _httpClient.DeleteAsync($"api/menu/{Uri.EscapeDataString(category)}/{Uri.EscapeDataString(id)}");
            response.EnsureSuccessStatusCode();
        }
    }
}
