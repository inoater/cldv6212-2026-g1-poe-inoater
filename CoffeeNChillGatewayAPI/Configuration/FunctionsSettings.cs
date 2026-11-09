using System.ComponentModel.DataAnnotations;

namespace CoffeeNChillGatewayAPI.Configuration
{
    public class FunctionsSettings  // Microsoft, 2026ab
    {
        public const string SectionName = "DownstreamServices";

        [Required]
        [Url]
        public string FunctionsBaseUrl { get; set; } = "http://localhost:7071";

        [Range(5, 300)]
        public int TimeoutSeconds { get; set; } = 30;
    }
}
