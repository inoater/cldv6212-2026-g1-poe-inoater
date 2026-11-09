using CoffeeNChillGatewayAPI.Clients;
using CoffeeNChillGatewayAPI.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoffeeNChillGatewayAPI.Controllers
{
    [ApiController]
    [Route("api/documents")]
    [Produces("application/json")]
    [Authorize(Roles = "Admin,Staff")]  // Microsoft, 2026x
    public class DocumentController : ControllerBase    // Microsoft, 2026ah; rudderz243, 2026
    {
        private readonly DocumentFunctionClient _documentClient;

        public DocumentController(DocumentFunctionClient documentClient)
        {
            _documentClient = documentClient;
        }

        /// <summary>
        /// Lists every staff document with its name, size and last modified date.
        /// </summary>
        /// <response code="200">The staff documents.</response>
        /// <response code="401">No valid token was sent.</response>
        /// <response code="403">Only Admin and Staff can view documents.</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<DocumentMetadataDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> ListFiles()
        {
            var result = await _documentClient.ListDocumentsAsync();
            return Ok(result);
        }

        /// <summary>
        /// Uploads a staff document (PDF, DOCX, TXT, PNG or JPG).
        /// </summary>
        /// <param name="file">The file to upload.</param>
        /// <response code="200">The document was uploaded.</response>
        /// <response code="400">No file was sent.</response>
        /// <response code="401">No valid token was sent.</response>
        /// <response code="403">Only Admin and Staff can upload documents.</response>
        /// <response code="415">The file type is not allowed.</response>
        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status415UnsupportedMediaType)]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file is null || file.Length is 0)
            {
                return BadRequest(new ErrorResponseDto { StatusCode = 400, Message = "File payload missing or empty" });
            }

            using var streamOfContent = file.OpenReadStream();     // W3Schools, 2026a
            await _documentClient.UploadDocumentAsync(file.FileName, streamOfContent, file.ContentType);
            return Ok(new { message = $"Staff document '{file.FileName}' uploaded successfully." });
        }

        /// <summary>
        /// Downloads a staff document.
        /// </summary>
        /// <param name="fileName">Name of the file, e.g. sample-recipe.pdf.</param>
        /// <response code="200">The file.</response>
        /// <response code="401">No valid token was sent.</response>
        /// <response code="403">Only Admin and Staff can download documents.</response>
        /// <response code="404">The file was not found.</response>
        [HttpGet("download/{fileName}")]
        [Produces("application/octet-stream")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Download([FromRoute] string fileName)
        {
            var (content, type) = await _documentClient.DownloadDocumentAsync(fileName);
            return File(content, type, fileName);
        }

        /// <summary>
        /// Deletes a staff document.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <response code="200">The file was deleted.</response>
        /// <response code="401">No valid token was sent.</response>
        /// <response code="403">Only Admin and Staff can delete documents.</response>
        /// <response code="404">The file was not found.</response>
        [HttpDelete("{fileName}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete([FromRoute] string fileName)
        {
            await _documentClient.DeleteDocumentAsync(fileName);
            return Ok(new { message = "Staff document deleted successfully." });
        }
    }
}
