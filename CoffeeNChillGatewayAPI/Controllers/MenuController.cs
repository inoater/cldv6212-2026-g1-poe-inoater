using CoffeeNChillGatewayAPI.Clients;
using CoffeeNChillGatewayAPI.Constants;
using CoffeeNChillGatewayAPI.DTOs;
using CoffeeNChillGatewayAPI.Mappers;
using CoffeeNChillGatewayAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoffeeNChillGatewayAPI.Controllers
{
    /// <summary>
    /// The CoffeeNChill menu, routed to the menu Azure Functions and the MenuItems table.
    /// </summary>
    [Route("api/menu")]
    public class MenuController : GatewayControllerBase     // Microsoft, 2026ah; rudderz243, 2026
    {
        private readonly MenuFunctionClient _menuClient;

        public MenuController(MenuFunctionClient menuClient)
        {
            _menuClient = menuClient;
        }

        /// <summary>
        /// Returns every item on the menu. No login required.
        /// </summary>
        /// <response code="200">The full menu.</response>
        /// <response code="502">The Functions app could not be reached.</response>
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(IEnumerable<MenuItemDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status502BadGateway)]
        public async Task<IActionResult> GetAll()
        {
            List<FunctionMenuItem> menuItems = await _menuClient.GetAllMenuItemsAsync();

            return Ok(menuItems.Select(item => item.ToDto()));
        }

        /// <summary>
        /// Returns the menu items in one category. No login required.
        /// </summary>
        /// <param name="category">Category name, e.g. "Hot Drinks".</param>
        /// <response code="200">The items in the category (empty when there are none).</response>
        /// <response code="502">The Functions app could not be reached.</response>
        [HttpGet("category/{category}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(IEnumerable<MenuItemDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status502BadGateway)]
        public async Task<IActionResult> GetByCategory([FromRoute] string category)
        {
            List<FunctionMenuItem> menuItems = await _menuClient.GetMenuItemsByCategoryAsync(category);

            return Ok(menuItems.Select(item => item.ToDto()));
        }

        /// <summary>
        /// Adds a new item to the menu.
        /// </summary>
        /// <param name="newItem">Category, SKU, name, description, price and availability.</param>
        /// <response code="201">The item was added.</response>
        /// <response code="400">The request body failed validation.</response>
        /// <response code="409">An item with that category and SKU already exists.</response>
        /// <response code="502">The Functions app could not be reached.</response>
        [HttpPost]
        [Authorize(Roles = Roles.AdminOrStaff)]     // Microsoft, 2026x
        [ProducesResponseType(typeof(MenuItemDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status502BadGateway)]
        public async Task<IActionResult> Create([FromBody] CreateMenuItemDto newItem)
        {
            FunctionMenuItem createdItem = await _menuClient.CreateMenuItemAsync(newItem.ToFunctionModel());

            MenuItemDto createdDto = createdItem.ToDto();

            return Created(
                $"/api/menu/category/{Uri.EscapeDataString(createdDto.Category)}",
                createdDto);   // Microsoft, 2026am
        }

        /// <summary>
        /// Updates the price, availability, name or description of a menu item.
        /// </summary>
        /// <remarks>Only the fields that are sent are changed.</remarks>
        /// <param name="category">Category of the item.</param>
        /// <param name="id">SKU of the item.</param>
        /// <param name="changes">The fields to change.</param>
        /// <response code="200">The item was updated.</response>
        /// <response code="400">The request body failed validation.</response>
        /// <response code="404">The item does not exist.</response>
        /// <response code="502">The Functions app could not be reached.</response>
        [HttpPut("{category}/{id}")]
        [Authorize(Roles = Roles.AdminOrStaff)]
        [ProducesResponseType(typeof(MenuItemDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status502BadGateway)]
        public async Task<IActionResult> Update(
            [FromRoute] string category,
            [FromRoute] string id,
            [FromBody] UpdateMenuItemDto changes)
        {
            FunctionMenuItem? existingItem = await _menuClient.GetMenuItemAsync(category, id);

            if (existingItem == null)
            {
                return ErrorResult(StatusCodes.Status404NotFound, "Menu item was not found.");
            }

            FunctionMenuItem updatedItem = await _menuClient.UpdateMenuItemAsync(
                existingItem.PartitionKey,
                existingItem.RowKey,
                existingItem.ApplyUpdate(changes));

            return Ok(updatedItem.ToDto());
        }

        /// <summary>
        /// Removes an item from the menu.
        /// </summary>
        /// <param name="category">Category of the item.</param>
        /// <param name="id">SKU of the item.</param>
        /// <response code="200">The item was deleted.</response>
        /// <response code="404">The item does not exist.</response>
        /// <response code="502">The Functions app could not be reached.</response>
        [HttpDelete("{category}/{id}")]
        [Authorize(Roles = Roles.AdminOrStaff)]
        [ProducesResponseType(typeof(MessageResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status502BadGateway)]
        public async Task<IActionResult> Delete([FromRoute] string category, [FromRoute] string id)
        {
            string functionMessage = await _menuClient.DeleteMenuItemAsync(category, id);

            return Ok(new MessageResponseDto { Message = functionMessage });
        }
    }
}
