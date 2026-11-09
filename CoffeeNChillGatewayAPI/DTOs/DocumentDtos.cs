using System.ComponentModel.DataAnnotations;

namespace CoffeeNChillGatewayAPI.DTOs
{
    /// <summary>
    /// A staff document stored in the staff-docs container.
    /// </summary>
    public record StaffDocumentDto  // rudderz243, 2026
    {
        /// <summary>File name of the document.</summary>
        public string FileName { get; init; } = string.Empty;

        /// <summary>File size in bytes.</summary>
        public long SizeBytes { get; init; }

        /// <summary>When the file was last modified (UTC).</summary>
        public DateTimeOffset? LastModified { get; init; }
    }

    /// <summary>
    /// A staff document upload sent as multipart/form-data.
    /// </summary>
    public class UploadStaffDocumentDto     // Microsoft, 2026i
    {
        /// <summary>The file to upload (PDF, DOCX, TXT, PNG or JPG, up to 10 MB).</summary>
        [Required]
        public IFormFile File { get; set; } = null!;
    }
}
