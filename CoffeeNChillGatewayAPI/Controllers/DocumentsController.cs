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
    /// Staff operational documents, routed to the document Azure Functions and the staff-docs container.
    /// </summary>
    [Route("api/documents")]
    [Authorize(Roles = Roles.AdminOrStaff)]     // Microsoft, 2026x
    public class DocumentsController : GatewayControllerBase    // Microsoft, 2026ah; rudderz243, 2026
    {
        private const long MaxUploadBytes = 10 * 1024 * 1024;

        private readonly DocumentFunctionClient _documentClient;

        public DocumentsController(DocumentFunctionClient documentClient)
        {
            _documentClient = documentClient;
        }

        /// <summary>
        /// Lists every staff document with its file name, size and last modified date.
        /// </summary>
        /// <response code="200">The stored documents.</response>
        /// <response code="502">The Functions app could not be reached.</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<StaffDocumentDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status502BadGateway)]
        public async Task<IActionResult> List()
        {
            List<FunctionStaffDocument> documents = await _documentClient.ListDocumentsAsync();

            return Ok(documents.Select(document => document.ToDto()));
        }

        /// <summary>
        /// Uploads a staff document (PDF, DOCX, TXT, PNG or JPG, up to 10 MB).
        /// </summary>
        /// <param name="upload">The file, sent as multipart/form-data under the "File" key.</param>
        /// <response code="200">The document was uploaded.</response>
        /// <response code="400">No file was attached or the file is empty.</response>
        /// <response code="409">A document with that file name already exists.</response>
        /// <response code="413">The file is larger than 10 MB.</response>
        /// <response code="415">The file type is not allowed.</response>
        /// <response code="502">The Functions app could not be reached.</response>
        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(MaxUploadBytes + 1024 * 1024)]    // Microsoft, 2026i
        [ProducesResponseType(typeof(MessageResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status413PayloadTooLarge)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status415UnsupportedMediaType)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status502BadGateway)]
        public async Task<IActionResult> Upload([FromForm] UploadStaffDocumentDto upload)
        {
            if (upload.File == null || upload.File.Length == 0)
            {
                return ErrorResult(StatusCodes.Status400BadRequest, "Please attach a file that is not empty.");
            }

            if (upload.File.Length > MaxUploadBytes)
            {
                return ErrorResult(StatusCodes.Status413PayloadTooLarge, "Staff documents cannot be larger than 10 MB.");
            }

            string fileName = Path.GetFileName(upload.File.FileName);

            using Stream fileStream = upload.File.OpenReadStream();     // W3Schools, 2026a

            string functionMessage = await _documentClient.UploadDocumentAsync(
                fileStream,
                fileName,
                upload.File.ContentType);

            return Ok(new MessageResponseDto { Message = functionMessage });
        }

        /// <summary>
        /// Streams a staff document back to the client.
        /// </summary>
        /// <param name="fileName">File name of the document, e.g. "sample-recipe.pdf".</param>
        /// <response code="200">The file contents.</response>
        /// <response code="404">The document does not exist.</response>
        /// <response code="502">The Functions app could not be reached.</response>
        [HttpGet("download/{fileName}")]
        [Produces("application/octet-stream", "application/json")]
        [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status502BadGateway)]
        public async Task<IActionResult> Download([FromRoute] string fileName)
        {
            DownloadedDocument download = await _documentClient.DownloadDocumentAsync(fileName);

            HttpContext.Response.RegisterForDispose(download);

            return File(download.Content, download.ContentType, Path.GetFileName(fileName));     // Microsoft, 2026k
        }

        /// <summary>
        /// Deletes a staff document.
        /// </summary>
        /// <param name="fileName">File name of the document.</param>
        /// <response code="200">The document was deleted.</response>
        /// <response code="404">The document does not exist.</response>
        /// <response code="502">The Functions app could not be reached.</response>
        [HttpDelete("{fileName}")]
        [ProducesResponseType(typeof(MessageResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status502BadGateway)]
        public async Task<IActionResult> Delete([FromRoute] string fileName)
        {
            string functionMessage = await _documentClient.DeleteDocumentAsync(fileName);

            return Ok(new MessageResponseDto { Message = functionMessage });
        }
    }
}
