using CoffeeNChillGatewayAPI.DTOs;
using System.Net.Http.Headers;

namespace CoffeeNChillGatewayAPI.Clients
{
    public class DocumentFunctionClient     // Microsoft, 2026aa; rudderz243, 2026
    {
        private readonly HttpClient _httpClient;

        public DocumentFunctionClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IEnumerable<DocumentMetadataDto>> ListDocumentsAsync()
        {
            var response = await _httpClient.GetAsync("api/documents");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<IEnumerable<DocumentMetadataDto>>() ?? new List<DocumentMetadataDto>();
        }

        public async Task<(Stream content, string contentType)> DownloadDocumentAsync(string fileName)
        {
            var response = await _httpClient.GetAsync($"api/documents/download/{Uri.EscapeDataString(fileName)}");
            response.EnsureSuccessStatusCode();

            string contentType = response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream";
            var streamOfContent = await response.Content.ReadAsStreamAsync();

            return (streamOfContent, contentType);
        }

        public async Task UploadDocumentAsync(string fileName, Stream content, string contentType)
        {
            using var formContent = new MultipartFormDataContent();     // Microsoft, 2026i
            using var fileContent = new StreamContent(content);

            fileContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            formContent.Add(fileContent, "file", fileName);

            var response = await _httpClient.PostAsync("api/documents/upload", formContent);
            response.EnsureSuccessStatusCode();
        }

        public async Task DeleteDocumentAsync(string fileName)
        {
            var response = await _httpClient.DeleteAsync($"api/documents/{Uri.EscapeDataString(fileName)}");
            response.EnsureSuccessStatusCode();
        }
    }
}
