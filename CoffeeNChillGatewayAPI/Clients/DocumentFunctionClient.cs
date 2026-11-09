using CoffeeNChillGatewayAPI.Models;
using System.Net.Http.Headers;

namespace CoffeeNChillGatewayAPI.Clients
{
    public class DocumentFunctionClient : FunctionClientBase    // Microsoft, 2026aa; rudderz243, 2026
    {
        public DocumentFunctionClient(HttpClient httpClient)
            : base(httpClient)
        {
        }

        public async Task<List<FunctionStaffDocument>> ListDocumentsAsync()
        {
            using HttpResponseMessage response = await _httpClient.GetAsync("api/documents");

            return await ReadFunctionJsonAsync<List<FunctionStaffDocument>>(response);
        }

        public async Task<string> UploadDocumentAsync(Stream fileStream, string fileName, string contentType)
        {
            using var formContent = new MultipartFormDataContent();     // Microsoft, 2026i
            using var fileContent = new StreamContent(fileStream);

            fileContent.Headers.ContentType = new MediaTypeHeaderValue(
                string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType);

            formContent.Add(fileContent, "file", fileName);

            using HttpResponseMessage response =
                await _httpClient.PostAsync("api/documents/upload", formContent);

            return await ReadFunctionMessageAsync(response);
        }

        public async Task<DownloadedDocument> DownloadDocumentAsync(string fileName)
        {
            HttpResponseMessage response = await _httpClient.GetAsync(
                $"api/documents/download/{Escape(fileName)}",
                HttpCompletionOption.ResponseHeadersRead);  // Microsoft, 2026aa

            try
            {
                await EnsureFunctionSuccessAsync(response);
            }
            catch
            {
                response.Dispose();
                throw;
            }

            string contentType =
                response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream";

            Stream fileStream = await response.Content.ReadAsStreamAsync();

            return new DownloadedDocument(fileStream, contentType, response);
        }

        public async Task<string> DeleteDocumentAsync(string fileName)
        {
            using HttpResponseMessage response =
                await _httpClient.DeleteAsync($"api/documents/{Escape(fileName)}");

            return await ReadFunctionMessageAsync(response);
        }
    }
}
