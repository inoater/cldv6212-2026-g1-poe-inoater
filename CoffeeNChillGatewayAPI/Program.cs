using CoffeeNChillGatewayAPI.Clients;
using CoffeeNChillGatewayAPI.Configuration;
using CoffeeNChillGatewayAPI.Controllers;
using CoffeeNChillGatewayAPI.Middleware;
using CoffeeNChillGatewayAPI.Services;
using CoffeeNChillGatewayAPI.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Reflection;
using System.Security.Claims;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOptions<JwtSettings>()      // Microsoft, 2026ab
    .Bind(builder.Configuration.GetSection(JwtSettings.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddOptions<FunctionsSettings>()
    .Bind(builder.Configuration.GetSection(FunctionsSettings.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.Configure<SeedAdminSettings>(builder.Configuration.GetSection(SeedAdminSettings.SectionName));

string connectionString =
    builder.Configuration.GetConnectionString("AzureStorage") ?? "UseDevelopmentStorage=true";

builder.Services.AddSingleton(await UserService.CreateAsync(connectionString, "Users"));
builder.Services.AddSingleton<TokenService>();
builder.Services.AddScoped<AuthService>();

builder.Services.AddHttpClient<MenuFunctionClient>(ConfigureFunctionsClient);      // Microsoft, 2026aa
builder.Services.AddHttpClient<DocumentFunctionClient>(ConfigureFunctionsClient);
builder.Services.AddHttpClient<OrderFunctionClient>(ConfigureFunctionsClient);

builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.PropertyNamingPolicy = null)
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>   // Microsoft, 2026ad
        {
            Dictionary<string, string[]> errors = context.ModelState
                .Where(entry => entry.Value?.Errors.Count > 0)
                .ToDictionary(
                    entry => entry.Key,
                    entry => entry.Value!.Errors.Select(error => error.ErrorMessage).ToArray());

            return new BadRequestObjectResult(ErrorResponseWriter.Create(
                context.HttpContext,
                StatusCodes.Status400BadRequest,
                "One or more fields are invalid.",
                errors: errors));
        };
    });

JwtSettings jwtSettings =
    builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>() ?? new JwtSettings();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>    // Microsoft, 2026w
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = TokenService.CreateSigningKey(jwtSettings.SecretKey),
            NameClaimType = ClaimTypes.Name,
            RoleClaimType = ClaimTypes.Role,
            ClockSkew = TimeSpan.FromMinutes(1)
        };

        options.Events = new JwtBearerEvents
        {
            OnChallenge = async context =>
            {
                context.HandleResponse();

                string message = context.AuthenticateFailure is SecurityTokenExpiredException
                    ? "Your token has expired. Please log in again."
                    : "A valid bearer token is required. Log in at POST /api/auth/login.";

                await ErrorResponseWriter.WriteAsync(context.HttpContext, StatusCodes.Status401Unauthorized, message);
            },
            OnForbidden = async context =>
            {
                await ErrorResponseWriter.WriteAsync(
                    context.HttpContext,
                    StatusCodes.Status403Forbidden,
                    "Your role is not allowed to do this.");
            }
        };
    });

builder.Services.AddAuthorization(options =>    // Microsoft, 2026x
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

builder.Services.AddRateLimiter(options =>      // Microsoft, 2026al
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddPolicy(AuthController.RateLimitPolicy, httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = builder.Configuration.GetValue("RateLimiting:AuthRequestsPerMinute", 30),
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));

    options.OnRejected = async (context, cancellationToken) =>
    {
        await ErrorResponseWriter.WriteAsync(
            context.HttpContext,
            StatusCodes.Status429TooManyRequests,
            "Too many login or register attempts. Please wait a minute and try again.");
    };
});

string[] allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();

builder.Services.AddCors(options =>     // Microsoft, 2026aj
{
    options.AddDefaultPolicy(policy =>
    {
        if (allowedOrigins.Length == 0)
        {
            policy.AllowAnyOrigin();
        }
        else
        {
            policy.WithOrigins(allowedOrigins);
        }

        policy.AllowAnyHeader().AllowAnyMethod();
    });
});

builder.Services.AddHealthChecks();     // Microsoft, 2026ai

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>   // Microsoft, 2026v; domaindrivendev, 2026
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "CoffeeNChill Gateway API",
        Version = "v3.0",
        Description = "Secure gateway for the CoffeeNChill canteen. Students can view the menu, place orders " +
                      "and track them without logging in. Baristas and canteen managers log in at " +
                      "POST /api/auth/login and use the token (Authorize button) to manage the menu, " +
                      "staff documents and order statuses."
    });

    string xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";

    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFile));    // Microsoft, 2026an

    options.AddSecurityDefinition(AuthorizeOperationFilter.SchemeName, new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Paste the Token value returned by POST /api/auth/login."
    });

    options.OperationFilter<AuthorizeOperationFilter>();
});

var app = builder.Build();

app.UseMiddleware<GlobalExceptionMiddleware>();     // Microsoft, 2026y

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "CoffeeNChill Gateway API v3.0");
    options.DocumentTitle = "CoffeeNChill Gateway API";
});

app.UseRouting();
app.UseCors();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health").AllowAnonymous();
app.MapGet("/", () => Results.Redirect("/swagger")).AllowAnonymous().ExcludeFromDescription();

using (IServiceScope scope = app.Services.CreateScope())
{
    AuthService authService = scope.ServiceProvider.GetRequiredService<AuthService>();
    SeedAdminSettings seedAdmin = scope.ServiceProvider.GetRequiredService<IOptions<SeedAdminSettings>>().Value;

    await authService.EnsureSeedAdminAsync(seedAdmin);
}

app.Run();

static void ConfigureFunctionsClient(IServiceProvider services, HttpClient httpClient)
{
    FunctionsSettings functionsSettings = services.GetRequiredService<IOptions<FunctionsSettings>>().Value;

    httpClient.BaseAddress = new Uri(functionsSettings.FunctionsBaseUrl.TrimEnd('/') + "/");
    httpClient.Timeout = TimeSpan.FromSeconds(functionsSettings.TimeoutSeconds);
}

public partial class Program
{
}
