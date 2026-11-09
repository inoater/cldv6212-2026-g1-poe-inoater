using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CoffeeNChillGatewayAPI.Swagger
{
    public class AuthorizeOperationFilter : IOperationFilter    // domaindrivendev, 2026; Microsoft, 2026v
    {
        public const string SchemeName = "Bearer";

        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            IList<object> endpointMetadata = context.ApiDescription.ActionDescriptor.EndpointMetadata;

            if (endpointMetadata.OfType<IAllowAnonymous>().Any())
            {
                if (endpointMetadata.OfType<AcceptsBearerTokenAttribute>().Any())
                {
                    AddBearerRequirement(operation, context);
                }

                return;
            }

            List<IAuthorizeData> authorizeData = endpointMetadata.OfType<IAuthorizeData>().ToList();

            if (authorizeData.Count == 0)
            {
                return;
            }

            operation.Responses ??= new OpenApiResponses();

            operation.Responses.TryAdd("401", new OpenApiResponse
            {
                Description = "Unauthorized - the bearer token is missing, invalid or expired."
            });

            string? roles = authorizeData
                .Select(a => a.Roles)
                .FirstOrDefault(r => !string.IsNullOrWhiteSpace(r));

            if (roles != null)
            {
                operation.Responses.TryAdd("403", new OpenApiResponse
                {
                    Description = $"Forbidden - only {roles.Replace(",", " or ")} users can do this."
                });

                operation.Description = string.IsNullOrWhiteSpace(operation.Description)
                    ? $"Requires role: {roles.Replace(",", " or ")}."
                    : $"{operation.Description}\n\nRequires role: {roles.Replace(",", " or ")}.";
            }

            AddBearerRequirement(operation, context);
        }

        private static void AddBearerRequirement(OpenApiOperation operation, OperationFilterContext context)
        {
            operation.Security ??= new List<OpenApiSecurityRequirement>();

            operation.Security.Add(new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference(SchemeName, context.Document)] = new List<string>()
            });
        }
    }
}
