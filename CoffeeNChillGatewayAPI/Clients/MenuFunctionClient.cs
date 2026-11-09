using CoffeeNChillGatewayAPI.Models;

namespace CoffeeNChillGatewayAPI.Clients
{
    public class MenuFunctionClient : FunctionClientBase    // Microsoft, 2026aa; rudderz243, 2026
    {
        public MenuFunctionClient(HttpClient httpClient)
            : base(httpClient)
        {
        }

        public async Task<List<FunctionMenuItem>> GetAllMenuItemsAsync()
        {
            using HttpResponseMessage response = await _httpClient.GetAsync("api/menu");

            return await ReadFunctionJsonAsync<List<FunctionMenuItem>>(response);
        }

        public async Task<List<FunctionMenuItem>> GetMenuItemsByCategoryAsync(string category)
        {
            using HttpResponseMessage response =
                await _httpClient.GetAsync($"api/menu/category/{Escape(category)}");

            return await ReadFunctionJsonAsync<List<FunctionMenuItem>>(response);
        }

        public async Task<FunctionMenuItem?> GetMenuItemAsync(string category, string id)
        {
            List<FunctionMenuItem> categoryItems = await GetMenuItemsByCategoryAsync(category);

            return categoryItems.FirstOrDefault(
                item => string.Equals(item.RowKey, id, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<FunctionMenuItem> CreateMenuItemAsync(FunctionMenuItem newItem)
        {
            using HttpResponseMessage response =
                await _httpClient.PostAsJsonAsync("api/menu", newItem, _functionJsonOptions);

            return await ReadFunctionJsonAsync<FunctionMenuItem>(response);
        }

        public async Task<FunctionMenuItem> UpdateMenuItemAsync(string category, string id, FunctionMenuItem updatedItem)
        {
            using HttpResponseMessage response = await _httpClient.PutAsJsonAsync(
                $"api/menu/{Escape(category)}/{Escape(id)}",
                updatedItem,
                _functionJsonOptions);

            return await ReadFunctionJsonAsync<FunctionMenuItem>(response);
        }

        public async Task<string> DeleteMenuItemAsync(string category, string id)
        {
            using HttpResponseMessage response =
                await _httpClient.DeleteAsync($"api/menu/{Escape(category)}/{Escape(id)}");

            return await ReadFunctionMessageAsync(response);
        }
    }
}
