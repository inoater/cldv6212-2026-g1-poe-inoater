namespace CoffeeNChillGatewayAPI.Models
{
    public sealed class DownloadedDocument : IDisposable
    {
        private readonly HttpResponseMessage _httpResponse;

        public DownloadedDocument(Stream content, string contentType, HttpResponseMessage httpResponse)
        {
            Content = content;
            ContentType = contentType;
            _httpResponse = httpResponse;
        }

        public Stream Content { get; }

        public string ContentType { get; }

        public void Dispose()
        {
            _httpResponse.Dispose();
        }
    }
}
