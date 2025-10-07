﻿using SemanticDocIngestor.Domain.Abstracts.Settings;
using SemanticDocIngestor.Domain.Contracts;
using SemanticDocIngestor.Domain.Options;
using SemanticDocIngestor.Core.Ollama;
using SemanticDocIngestor.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Ollama;

namespace SemanticDocIngestor.Core
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registers all core SemanticDocIngestor services, including:
        /// - Ollama API client
        /// - Conversation & model management (via <see cref="IOllamaFactory"/>)
        /// - Web scraping support (<see cref="IOllamaScraper"/>)
        /// - AutoMapper profiles
        /// - Core middleware pipeline
        /// - Optional APM integration
        /// </summary>
        /// <param name="services">The service collection to register into.</param>
        /// <param name="configuration">Application configuration (e.g., from appsettings.json).</param>
        /// <param name="useApm">Enable APM logging and diagnostics (optional).</param>
        /// <returns>The modified service collection for chaining.</returns>
        public static IServiceCollection AddSemanticDocIngestorCore(this IServiceCollection services, IConfiguration configuration, bool useApm = false)
        {
            // Bind OllamaOptions from configuration
            OllamaOptions options = new();
            services.Configure<OllamaOptions>(configuration.GetSection(nameof(OllamaOptions)));
            configuration.GetSection(nameof(OllamaOptions)).Bind(options);

            // Register Middleware-related services
            services.AddSemanticDocIngestorMiddleware(configuration, useApm: useApm);

            // Register singleton Ollama API client with base endpoint from config
            services.AddSingleton<IOllamaApiClient>(factory =>
            {
                var uri = configuration.GetConnectionString("ollama")?.Replace("Endpoint=", "") + "/api";
                return new OllamaApiClient(baseUri: new Uri(uri));
            });

            // Register core factory and scraping services
            services.AddScoped<IOllamaScraper, OllamaScraper>();
            services.AddScoped<IOllamaFactory, OllamaFactory>();

            // Register AutoMapper profiles across all relevant assemblies
            services.AddAutoMapper(cfg =>
            {
                cfg.AddMaps(typeof(AppSettingsBase).Assembly,
                            typeof(Persistence.ServiceCollectionExtensions).Assembly,
                            typeof(SemanticDocIngestor.Core.ServiceCollectionExtensions).Assembly,
                            typeof(SemanticDocIngestor.Middleware.ServiceCollectionExtensions).Assembly);
            });

            return services;
        }

        /// <summary>
        /// Adds SemanticDocIngestor's middleware pipeline to the application. 
        /// This includes exception handling, logging, resilience strategies (retry, timeout, circuit breaker),
        /// and any registered APM/logging integrations.
        /// </summary>
        /// <param name="app">The application pipeline builder.</param>
        /// <param name="configuration">Application configuration (e.g., for APM or environment settings).</param>
        /// <param name="loggerFactory">Logger factory to be used for diagnostics and middleware logging.</param>
        /// <returns>The application builder for chaining.</returns>
        public static IApplicationBuilder UseSemanticDocIngestorCore(this IApplicationBuilder app, IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            // Plug in middleware pipeline (from SemanticDocIngestor.Middleware)
            app.UseSemanticDocIngestorMiddleware(configuration, loggerFactory);

            return app;
        }

    }
}
