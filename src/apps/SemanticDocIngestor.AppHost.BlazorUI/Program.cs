using SemanticDocIngestor.AppHost.BlazorUI.Components;
using SemanticDocIngestor.AppHost.BlazorUI.Services;
using SemanticDocIngestor.AppHost.ServiceDefaults;
using SemanticDocIngestor.Core;
using SemanticDocIngestor.Middleware.Serilog;
using Serilog;
using Serilog.Core;
using System.Configuration;

var builder = WebApplication.CreateBuilder(args);
Log.Logger = SemanticDocIngestorLoggingExtensions.AddSemanticDocIngestorLogging(builder.Configuration);

builder.Services.AddControllers();

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

builder.Services.AddBlazorBootstrap();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();


builder.Services.AddSemanticDocIngestorCore(builder.Configuration, useApm: false);
builder.Services.AddScoped<OllamaService>();
builder.Services.AddScoped<ChatSidebarUpdateService>();
builder.Services.AddHttpClient();

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

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

var loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();
app.UseSemanticDocIngestorCore(app.Configuration, loggerFactory);

app.UseHttpsRedirection();

app.UseAntiforgery();

//app.MapStaticAssets();
app.UseStaticFiles();

app.MapControllers();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapDefaultEndpoints();

app.Run();
