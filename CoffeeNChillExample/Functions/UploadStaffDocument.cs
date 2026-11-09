using Azure;
using CoffeeNChill.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System;

namespace CoffeeNChill.Functions
{
    public class UploadStaffDocument
    {
        private static readonly Dictionary<string, string> AllowedTypes =
            new(StringComparer.OrdinalIgnoreCase)
            {
                [".pdf"] = "application/pdf",
                [".docx"] = "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                [".txt"] = "text/plain",
                [".png"] = "image/png",
                [".jpg"] = "image/jpeg",
                [".jpeg"] = "image/jpeg"
            };

        private readonly ILogger<UploadStaffDocument> _logger;
        private readonly BlobService _blobService;

        public UploadStaffDocument(
            ILogger<UploadStaffDocument> logger,
            BlobService blobService)
        {
            _logger = logger;
            _blobService = blobService;
        }

        [Function("UploadStaffDocument")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "documents/upload")]
            HttpRequest req)
        {
            _logger.LogInformation("Uploading a staff document.");

            try
            {
                if (!req.HasFormContentType || req.Form.Files.Count == 0)   // Microsoft, 2026i
                {
                    return new BadRequestObjectResult(
                        "Please attach a file using multipart/form-data with a 'file' key.");
                }

                IFormFile file = req.Form.Files[0];

                if (file.Length == 0)
                {
                    return new BadRequestObjectResult("The uploaded file is empty.");
                }

                string fileName = Path.GetFileName(file.FileName);
                string extension = Path.GetExtension(fileName);

                if (!AllowedTypes.TryGetValue(extension, out string? expectedType) ||
                    !string.Equals(file.ContentType, expectedType, StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning(
                        "Rejected upload '{fileName}' with content type '{contentType}'.",
                        fileName,
                        file.ContentType);

                    return new ObjectResult(
                        "Unsupported file type. Allowed types: PDF, DOCX, TXT, PNG and JPG.")
                    {
                        StatusCode = StatusCodes.Status415UnsupportedMediaType
                    };
                }

                using (Stream fileStream = file.OpenReadStream())   // W3Schools, 2026a
                {
                    await _blobService.UploadFileAsync(
                        fileStream,
                        fileName,
                        expectedType);
                }

                return new OkObjectResult(
                    $"Staff document '{fileName}' uploaded successfully.");
            }
            catch (RequestFailedException e) when (e.Status == StatusCodes.Status409Conflict)
            {
                _logger.LogWarning(e, "A staff document with that name already exists.");

                return new ObjectResult(
                    "A document with that file name already exists.")
                {
                    StatusCode = StatusCodes.Status409Conflict
                };
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error uploading staff document.");

                return new ObjectResult(
                    "An error occurred while uploading the document.")
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };  // IIE, 2026
            }
        }
    }
}