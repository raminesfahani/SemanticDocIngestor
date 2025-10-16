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
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
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
app.UseCors();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}


var loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();
app.UseSemanticDocIngestorCore(app.Configuration, loggerFactory);

app.UseStaticFiles();
app.MapDefaultEndpoints();
app.MapControllers();

app.Run();
