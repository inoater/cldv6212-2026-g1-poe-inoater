using CoffeeNChill.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;

namespace CoffeeNChill.Functions
{
    public class DeleteStaffDocument
    {
        private readonly ILogger<DeleteStaffDocument> _logger;
        private readonly BlobService _blobService;

        public DeleteStaffDocument(
            ILogger<DeleteStaffDocument> logger,
            BlobService blobService)
        {
            _logger = logger;
            _blobService = blobService;
        }

        [Function("DeleteStaffDocument")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(
                AuthorizationLevel.Anonymous,
                "delete",
                Route = "documents/{fileName}")]    // Microsoft, 2026c
            HttpRequestData req,
            string fileName)
        {
            _logger.LogInformation("Deleting staff document {fileName}.", fileName);

            try
            {
                if (string.IsNullOrWhiteSpace(fileName))
                {
                    var errorResponse =
                        req.CreateResponse(HttpStatusCode.BadRequest);

                    await errorResponse.WriteStringAsync(
                        "File name is required.");

                    return errorResponse;
                }

                bool deleted =
                    await _blobService.DeleteFileAsync(Path.GetFileName(fileName));

                if (!deleted)
                {
                    var errorResponse =
                        req.CreateResponse(HttpStatusCode.NotFound);

                    await errorResponse.WriteStringAsync(
                        "File was not found.");

                    return errorResponse;
                }

                var response =
                    req.CreateResponse(HttpStatusCode.OK);

                await response.WriteStringAsync(
                    "Staff document deleted successfully.");

                return response;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error deleting staff document.");

                var response =
                    req.CreateResponse(
                        HttpStatusCode.InternalServerError);

                await response.WriteStringAsync(
                    "An error occurred while deleting the document.");

                return response;    // IIE, 2026
            }
        }
    }
}
