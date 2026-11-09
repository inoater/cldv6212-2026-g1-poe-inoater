using CoffeeNChillGatewayAPI.DTOs;
using System.ComponentModel.DataAnnotations;

namespace CoffeeNChillGatewayAPI.Tests
{
    public class DtoValidationTests     // Microsoft, 2026ae; Microsoft, 2026ad
    {
        [Fact]
        public void RegisterRequest_WithAStrongPasswordIsValid()
        {
            var newUser = new RegisterRequestDto
            {
                Email = "barista@coffeenchill.ac.za",
                Password = "Barista@2026",
                FullName = "Busi Barista",
                Role = "Staff"
            };

            Assert.Empty(Validate(newUser));
        }

        [Theory]
        [InlineData("short1A")]
        [InlineData("alllowercase1")]
        [InlineData("NoNumbersHere")]
        public void RegisterRequest_WithAWeakPasswordIsInvalid(string weakPassword)
        {
            var newUser = new RegisterRequestDto
            {
                Email = "student@coffeenchill.ac.za",
                Password = weakPassword,
                FullName = "Sam Student"
            };

            Assert.Contains(Validate(newUser), result => result.MemberNames.Contains(nameof(RegisterRequestDto.Password)));
        }

        [Fact]
        public void RegisterRequest_WithAnUnknownRoleIsInvalid()
        {
            var newUser = new RegisterRequestDto
            {
                Email = "student@coffeenchill.ac.za",
                Password = "Student@2026",
                FullName = "Sam Student",
                Role = "Manager"
            };

            Assert.Contains(Validate(newUser), result => result.MemberNames.Contains(nameof(RegisterRequestDto.Role)));
        }

        [Fact]
        public void PlaceOrder_WithNoItemsOrZeroTotalIsInvalid()
        {
            var newOrder = new PlaceOrderDto
            {
                CustomerName = "Jane Smith",
                SelectedItemSKUs = new List<string>(),
                TotalPrice = 0
            };

            List<ValidationResult> results = Validate(newOrder);

            Assert.Contains(results, result => result.MemberNames.Contains(nameof(PlaceOrderDto.SelectedItemSKUs)));
            Assert.Contains(results, result => result.MemberNames.Contains(nameof(PlaceOrderDto.TotalPrice)));
        }

        [Fact]
        public void UpdateOrderStatus_OnlyAcceptsTheBaristaLifecycleStatuses()
        {
            Assert.Empty(Validate(new UpdateOrderStatusDto { OrderId = "ORD-2026-8801", Status = "Ready" }));
            Assert.NotEmpty(Validate(new UpdateOrderStatusDto { OrderId = "ORD-2026-8801", Status = "Burnt" }));
        }

        [Fact]
        public void UpdateMenuItem_WithNoFieldsIsInvalid()
        {
            Assert.NotEmpty(Validate(new UpdateMenuItemDto()));
            Assert.Empty(Validate(new UpdateMenuItemDto { IsAvailable = false }));
        }

        [Fact]
        public void CreateMenuItem_WithANegativePriceIsInvalid()
        {
            var newItem = new CreateMenuItemDto
            {
                Category = "Hot Drinks",
                Id = "COF-001",
                Name = "Espresso",
                Price = -5
            };

            Assert.Contains(Validate(newItem), result => result.MemberNames.Contains(nameof(CreateMenuItemDto.Price)));
        }

        private static List<ValidationResult> Validate(object model)
        {
            var results = new List<ValidationResult>();

            Validator.TryValidateObject(model, new ValidationContext(model), results, validateAllProperties: true);

            return results;
        }
    }
}
