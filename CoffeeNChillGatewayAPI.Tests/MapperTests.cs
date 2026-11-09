using CoffeeNChillGatewayAPI.DTOs;
using CoffeeNChillGatewayAPI.Mappers;
using CoffeeNChillGatewayAPI.Models;

namespace CoffeeNChillGatewayAPI.Tests
{
    public class MapperTests    // Microsoft, 2026ae; Microsoft, 2026ac
    {
        [Fact]
        public void MenuItemToDto_MapsPartitionKeyAndRowKeyToCategoryAndId()
        {
            var menuItem = new FunctionMenuItem
            {
                PartitionKey = "Hot Drinks",
                RowKey = "COF-001",
                Name = "Espresso",
                Price = 25.0,
                IsAvailable = true
            };

            MenuItemDto menuItemDto = menuItem.ToDto();

            Assert.Equal("Hot Drinks", menuItemDto.Category);
            Assert.Equal("COF-001", menuItemDto.Id);
            Assert.Equal("Espresso", menuItemDto.Name);
            Assert.Equal(25.0, menuItemDto.Price);
        }

        [Fact]
        public void CreateMenuItemDto_IsTrimmedAndSkuIsUpperCase()
        {
            var newItem = new CreateMenuItemDto
            {
                Category = " Pastries ",
                Id = "pas-104",
                Name = " Ham & Cheese Croissant ",
                Price = 40.0
            };

            FunctionMenuItem functionItem = newItem.ToFunctionModel();

            Assert.Equal("Pastries", functionItem.PartitionKey);
            Assert.Equal("PAS-104", functionItem.RowKey);
            Assert.Equal("Ham & Cheese Croissant", functionItem.Name);
        }

        [Fact]
        public void ApplyUpdate_OnlyChangesTheFieldsThatWereSent()
        {
            var existingItem = new FunctionMenuItem
            {
                PartitionKey = "Hot Drinks",
                RowKey = "COF-001",
                Name = "Espresso",
                Description = "Single shot",
                Price = 25.0,
                IsAvailable = true
            };

            FunctionMenuItem updatedItem = existingItem.ApplyUpdate(new UpdateMenuItemDto
            {
                Price = 28.5,
                IsAvailable = false
            });

            Assert.Equal("Espresso", updatedItem.Name);
            Assert.Equal("Single shot", updatedItem.Description);
            Assert.Equal(28.5, updatedItem.Price);
            Assert.False(updatedItem.IsAvailable);
        }

        [Fact]
        public void OrderToDto_SplitsTheStoredSkuList()
        {
            var order = new FunctionOrder
            {
                PartitionKey = "2026-11-09",
                RowKey = "ORD-2026-8801",
                CustomerName = "Jane Smith",
                SelectedItemSKUs = "COF-001, PAS-104",
                TotalPrice = 65.0,
                Status = "Preparing"
            };

            OrderDto orderDto = order.ToDto();

            Assert.Equal("ORD-2026-8801", orderDto.OrderId);
            Assert.Equal("2026-11-09", orderDto.OrderDate);
            Assert.Equal(new List<string> { "COF-001", "PAS-104" }, orderDto.SelectedItemSKUs);
            Assert.Equal("Preparing", orderDto.Status);
        }

        [Fact]
        public void PlaceOrderDto_WithoutOrderIdLeavesItForTheFunctionToGenerate()
        {
            var newOrder = new PlaceOrderDto
            {
                OrderId = "  ",
                CustomerName = " Jane Smith ",
                SelectedItemSKUs = new List<string> { " COF-001 " },
                TotalPrice = 25.0
            };

            FunctionOrderMessage functionMessage = newOrder.ToFunctionMessage();

            Assert.Null(functionMessage.OrderId);
            Assert.Null(functionMessage.OrderTimestamp);
            Assert.Equal("Jane Smith", functionMessage.CustomerName);
            Assert.Equal("COF-001", functionMessage.SelectedItemSKUs.Single());
        }
    }
}
