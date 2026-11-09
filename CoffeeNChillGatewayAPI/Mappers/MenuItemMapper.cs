using CoffeeNChillGatewayAPI.DTOs;
using CoffeeNChillGatewayAPI.Models;

namespace CoffeeNChillGatewayAPI.Mappers
{
    public static class MenuItemMapper  // Microsoft, 2026ac
    {
        public static MenuItemDto ToDto(this FunctionMenuItem menuItem)
        {
            return new MenuItemDto
            {
                Category = menuItem.PartitionKey,
                Id = menuItem.RowKey,
                Name = menuItem.Name,
                Description = menuItem.Description,
                Price = menuItem.Price,
                IsAvailable = menuItem.IsAvailable,
                LastUpdated = menuItem.Timestamp
            };
        }

        public static FunctionMenuItem ToFunctionModel(this CreateMenuItemDto newItem)
        {
            return new FunctionMenuItem
            {
                PartitionKey = newItem.Category.Trim(),
                RowKey = newItem.Id.Trim().ToUpperInvariant(),
                Name = newItem.Name.Trim(),
                Description = newItem.Description?.Trim() ?? string.Empty,
                Price = newItem.Price,
                IsAvailable = newItem.IsAvailable
            };
        }

        public static FunctionMenuItem ApplyUpdate(this FunctionMenuItem existingItem, UpdateMenuItemDto changes)
        {
            return new FunctionMenuItem
            {
                PartitionKey = existingItem.PartitionKey,
                RowKey = existingItem.RowKey,
                Name = changes.Name?.Trim() ?? existingItem.Name,
                Description = changes.Description?.Trim() ?? existingItem.Description,
                Price = changes.Price ?? existingItem.Price,
                IsAvailable = changes.IsAvailable ?? existingItem.IsAvailable
            };
        }
    }
}
