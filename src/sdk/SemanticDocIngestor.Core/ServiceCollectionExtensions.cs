using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SemanticDocIngestor.Core.Services;
using SemanticDocIngestor.Domain.Abstractions.Services;
using SemanticDocIngestor.Domain.Abstractions.Settings;
using SemanticDocIngestor.Infrastructure;
using SemanticDocIngestor.Infrastructure.Configurations;

namespace SemanticDocIngestor.Core
{
    /// <summary>
    /// Extension methods for configuring SemanticDocIngestor services in the dependency injection container.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registers AutoMapper profiles for all SemanticDocIngestor assemblies.
        /// Scans and adds mapping configurations from Domain, Core, and Infrastructure layers.
        /// </summary>
        /// <param name="services">The service collection to add mappings to.</param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection RegisterMappingProfiles(this IServiceCollection services)
        {
            services.AddAutoMapper(cfg =>
            {
                cfg.AddMaps(typeof(AppSettings).Assembly,
                            typeof(SemanticDocIngestor.Domain.ServiceCollectionExtensions).Assembly,
                            typeof(SemanticDocIngestor.Core.ServiceCollectionExtensions).Assembly,
                            typeof(SemanticDocIngestor.Infrastructure.ServiceCollectionExtensions).Assembly);
            });

            return services;
        }

        /// <summary>
        /// Adds all SemanticDocIngestor Core services to the dependency injection container.
        /// This includes document processing, vector stores, keyword search, RAG services, and more.
        /// </summary>
        /// <param name="services">The service collection to add services to.</param>
        /// <param name="configuration">Application configuration containing connection strings and settings.</param>
        /// <returns>The service collection for chaining.</returns>
        /// <remarks>
        /// This method registers:
        /// - Document ingestor service (singleton)
        /// - Infrastructure services (Elasticsearch, Qdrant, Ollama)
        /// - AutoMapper profiles
        /// - Resilience policies
        /// - Middleware components
        /// </remarks>
        public static IServiceCollection AddSemanticDocIngestorCore(this IServiceCollection services, IConfiguration configuration)
        {
            services.RegisterMappingProfiles()
                    .AddInfrastructure(configuration);

            services.AddSingleton<IDocumentIngestorService, DocumentIngestorService>();

            return services;
        }

        /// <summary>
        /// Configures the application to use SemanticDocIngestor middleware components.
        /// This includes resilience, logging, and exception handling middleware.
        /// </summary>
        /// <param name="app">The application builder to configure.</param>
        /// <param name="Configuration">Application configuration.</param>
        /// <param name="loggerFactory">Logger factory for creating loggers in middleware.</param>
        /// <returns>The application builder for chaining.</returns>
        public static IApplicationBuilder UseSemanticDocIngestorCore(this IApplicationBuilder app, IConfiguration Configuration, ILoggerFactory loggerFactory)
        {
            app.UseInfrastructure(Configuration, loggerFactory);

            return app;
        }
    }
}
