using CoffeeNChillGatewayAPI.Clients;
using CoffeeNChillGatewayAPI.Middleware;
using CoffeeNChillGatewayAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

string connectionString = builder.Configuration.GetConnectionString("AzureStorage") ?? "UseDevelopmentStorage=true";    // Microsoft, 2026al

builder.Services.AddSingleton(await UserService.CreateAsync(connectionString, "Users"));
builder.Services.AddScoped<AuthService>();

var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secret = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!);

builder.Services.AddAuthentication(options =>     // Microsoft, 2026w; rudderz243, 2026
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(secret)
    };
});

builder.Services.AddAuthorization();    // Microsoft, 2026x

builder.Services.AddSwaggerGen(options =>   // Microsoft, 2026v; rudderz243, 2026
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "CoffeeNChill Gateway API",
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter JWT Token"
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = new List<string>()
    });
});

Uri functionUri = new Uri(builder.Configuration["DownstreamServices:FunctionsBaseUrl"] ?? "http://localhost:7071");

builder.Services.AddHttpClient<MenuFunctionClient>(c => c.BaseAddress = functionUri);   // Microsoft, 2026aa; rudderz243, 2026
builder.Services.AddHttpClient<DocumentFunctionClient>(c => c.BaseAddress = functionUri);
builder.Services.AddHttpClient<OrderFunctionClient>(c => c.BaseAddress = functionUri);

var app = builder.Build();

app.UseMiddleware<GlobalExceptionMiddleware>();     // Microsoft, 2026y

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "CoffeeNChill Gateway API");
    });
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
