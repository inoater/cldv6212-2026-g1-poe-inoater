using CoffeeNChillGatewayAPI.Clients;
using CoffeeNChillGatewayAPI.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoffeeNChillGatewayAPI.Controllers
{
    [ApiController]
    [Route("api/menu")]
    [Produces("application/json")]
    public class MenuController : ControllerBase    // Microsoft, 2026ah; rudderz243, 2026
    {
        private readonly MenuFunctionClient _menuClient;

        public MenuController(MenuFunctionClient menuClient)
        {
            _menuClient = menuClient;
        }

        /// <summary>
        /// Gets every item on the menu.
        /// </summary>
        /// <response code="200">The menu items.</response>
        /// <response code="502">The Functions app could not be reached.</response>
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(IEnumerable<MenuItemResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status502BadGateway)]
        public async Task<IActionResult> GetAll()
        {
            var result = await _menuClient.GetAllMenuItemsAsync();
            return Ok(result);
        }

        /// <summary>
        /// Gets the menu items in one category.
        /// </summary>
        /// <param name="category">Category, e.g. Hot Drinks.</param>
        /// <response code="200">The menu items in the category.</response>
        /// <response code="502">The Functions app could not be reached.</response>
        [HttpGet("category/{category}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(IEnumerable<MenuItemResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status502BadGateway)]
        public async Task<IActionResult> GetByCategory([FromRoute] string category)
        {
            var result = await _menuClient.GetMenuItemsByCategoryAsync(category);
            return Ok(result);
        }

        /// <summary>
        /// Adds a new item to the menu.
        /// </summary>
        /// <param name="dto">Category (PartitionKey), SKU (RowKey), name, description, price and availability.</param>
        /// <response code="201">The item was added.</response>
        /// <response code="400">The item details are invalid.</response>
        /// <response code="401">No valid token was sent.</response>
        /// <response code="403">Only Admin and Staff can add items.</response>
        /// <response code="409">The item already exists.</response>
        [HttpPost]
        [Authorize(Roles = "Admin,Staff")]  // Microsoft, 2026x
        [ProducesResponseType(typeof(MenuItemResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Create([FromBody] MenuItemCreateRequestDto dto)
        {
            var result = await _menuClient.CreateMenuItemAsync(dto);
            return StatusCode(StatusCodes.Status201Created, result);
        }

        /// <summary>
        /// Updates the price or availability of a menu item.
        /// </summary>
        /// <param name="category">Category of the item.</param>
        /// <param name="id">SKU of the item.</param>
        /// <param name="dto">The new name, description, price and availability.</param>
        /// <response code="200">The item was updated.</response>
        /// <response code="400">The item details are invalid.</response>
        /// <response code="401">No valid token was sent.</response>
        /// <response code="403">Only Admin and Staff can update items.</response>
        /// <response code="404">The item was not found.</response>
        [HttpPut("{category}/{id}")]
        [Authorize(Roles = "Admin,Staff")]
        [ProducesResponseType(typeof(MenuItemResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update([FromRoute] string category, [FromRoute] string id, [FromBody] MenuItemUpdateRequestDto dto)
        {
            var result = await _menuClient.UpdateMenuItemAsync(category, id, dto);
            return Ok(result);
        }

        /// <summary>
        /// Removes an item from the menu.
        /// </summary>
        /// <param name="category">Category of the item.</param>
        /// <param name="id">SKU of the item.</param>
        /// <response code="200">The item was deleted.</response>
        /// <response code="401">No valid token was sent.</response>
        /// <response code="403">Only Admin and Staff can delete items.</response>
        /// <response code="404">The item was not found.</response>
        [HttpDelete("{category}/{id}")]
        [Authorize(Roles = "Admin,Staff")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete([FromRoute] string category, [FromRoute] string id)
        {
            await _menuClient.DeleteMenuItemAsync(category, id);
            return Ok(new { message = "Menu item deleted successfully." });
        }
    }
}
