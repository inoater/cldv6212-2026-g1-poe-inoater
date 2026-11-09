using CoffeeNChillGatewayAPI.Exceptions;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CoffeeNChillGatewayAPI.Clients
{
    public abstract class FunctionClientBase    // Microsoft, 2026aa; W3Schools, 2026f
    {
        protected static readonly JsonSerializerOptions _functionJsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = null,
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        protected readonly HttpClient _httpClient;

        protected FunctionClientBase(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        protected static string Escape(string value)
        {
            return Uri.EscapeDataString(value);
        }

        protected static async Task EnsureFunctionSuccessAsync(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
            {
                return;
            }

            string functionMessage = await response.Content.ReadAsStringAsync();

            functionMessage = functionMessage.Trim().Trim('"');

            if (string.IsNullOrWhiteSpace(functionMessage))
            {
                functionMessage = response.ReasonPhrase ?? "The Functions app returned an error.";
            }

            throw new FunctionsApiException(response.StatusCode, functionMessage);
        }

        protected static async Task<T> ReadFunctionJsonAsync<T>(HttpResponseMessage response)
            where T : new()
        {
            await EnsureFunctionSuccessAsync(response);

            T? result = await response.Content.ReadFromJsonAsync<T>(_functionJsonOptions);  // W3Schools, 2026b

            return result ?? new T();
        }

        protected static async Task<string> ReadFunctionMessageAsync(HttpResponseMessage response)
        {
            await EnsureFunctionSuccessAsync(response);

            string functionMessage = await response.Content.ReadAsStringAsync();

            return functionMessage.Trim().Trim('"');
        }
    }
}
