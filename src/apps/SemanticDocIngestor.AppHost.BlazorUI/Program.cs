using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Identity.Web;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using SemanticDocIngestor.AppHost.BlazorUI.Components;
using SemanticDocIngestor.AppHost.BlazorUI.Services;
using SemanticDocIngestor.AppHost.ServiceDefaults;
using SemanticDocIngestor.Core;
using SemanticDocIngestor.Infrastructure.Middlewares;
using Serilog;
using Serilog.Core;
using System.Configuration;
using SemanticDocIngestor.Infrastructure.Factories.Docs;
using Microsoft.AspNetCore.Authentication.OAuth;

var builder = WebApplication.CreateBuilder(args);
Log.Logger = SemanticDocIngestorLoggingExtensions.AddSerilogLogging(builder.Configuration);

// AuthN
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme; // default to Microsoft
    })
    .AddCookie()
    .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"))
        .EnableTokenAcquisitionToCallDownstreamApi()
        .AddMicrosoftGraph(defaultScopes: "Files.Read")
        .AddInMemoryTokenCaches()
    .Services
    .AddAuthentication()
    .AddGoogle("Google", options =>
    {
        options.ClientId = builder.Configuration["Google:ClientId"]!;
        options.ClientSecret = builder.Configuration["Google:ClientSecret"]!;
        options.Scope.Add("openid");
        options.Scope.Add("email");
        options.Scope.Add("profile");
        options.Scope.Add("https://www.googleapis.com/auth/drive.readonly");
        options.SaveTokens = true;
        options.AccessType = "offline";

        // Append prompt=consent when redirecting to Google
        options.Events ??= new OAuthEvents();
        options.Events.OnRedirectToAuthorizationEndpoint = context =>
        {
            var separator = context.RedirectUri.Contains('?') ? "&" : "?";
            context.Response.Redirect($"{context.RedirectUri}{separator}prompt=consent");
            return Task.CompletedTask;
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddBlazorBootstrap();

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();


builder.Services.AddSemanticDocIngestorCore(builder.Configuration);
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

// Services to call user-scoped clouds
builder.Services.AddScoped<UserCloudFileService>();

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

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
