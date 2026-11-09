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

        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(IEnumerable<MenuItemResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status502BadGateway)]
        public async Task<IActionResult> GetAll()
        {
            var result = await _menuClient.GetAllMenuItemsAsync();
            return Ok(result);
        }

        [HttpGet("category/{category}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(IEnumerable<MenuItemResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status502BadGateway)]
        public async Task<IActionResult> GetByCategory([FromRoute] string category)
        {
            var result = await _menuClient.GetMenuItemsByCategoryAsync(category);
            return Ok(result);
        }

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
