using SemanticDocIngestor.AppHost.ServiceDefaults;
using SemanticDocIngestor.Core;
using Microsoft.Extensions.Options;
using Scalar.AspNetCore;
using Serilog;
using SemanticDocIngestor.Infrastructure.Middlewares;

var builder = WebApplication.CreateBuilder(args);
Log.Logger = SemanticDocIngestorLoggingExtensions.AddSerilogLogging(builder.Configuration);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();

builder.Services.AddControllers();

// Add HttpContextAccessor for dynamic URL resolution
builder.Services.AddHttpContextAccessor();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        // Try to get the current HTTP context to build dynamic URL
        var httpContext = context.ApplicationServices
            .GetService<IHttpContextAccessor>()?.HttpContext;

        if (httpContext != null)
        {
            // Build URL from current request (works for Aspire with dynamic ports)
            var scheme = httpContext.Request.Scheme;
            var host = httpContext.Request.Host.ToString();
            var baseUrl = $"{scheme}://{host}";

            document.Servers =
            [
                new() { Url = baseUrl, Description = "Current Server" }
            ];
        }
        else
        {
            // Fallback for Docker Compose (used during app startup before first request)
            document.Servers =
            [
                new() { Url = "http://localhost:5001", Description = "Docker Compose" }
            ];
        }

        return Task.CompletedTask;
    });
});

builder.Services.AddSemanticDocIngestorCore(builder.Configuration);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

// Enable OpenAPI and Scalar in Development OR when running in containers
if (app.Environment.IsDevelopment() || !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER")))
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options
  .WithTitle("SemanticDocIngestor API")
            .WithTheme(ScalarTheme.Purple)
            .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
    });
}

// CORS must be before routing
app.UseCors();

var loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();
app.UseSemanticDocIngestorCore(app.Configuration, loggerFactory);

app.UseStaticFiles();
app.MapDefaultEndpoints();
app.MapControllers();

app.Run();
