using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SemanticDocIngestor.Infrastructure.Configurations;
using SemanticDocIngestor.Infrastructure.Factories.Docs;
using SemanticDocIngestor.Infrastructure.Factories.Ollama;
using SemanticDocIngestor.Infrastructure.Middlewares;
using SemanticDocIngestor.Infrastructure.Persistence;
using Serilog;

namespace SemanticDocIngestor.Infrastructure
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration, bool useApm = false)
        {
            // Configure AppSettings
            services.Configure<AppSettings>(settings => configuration.GetSection(nameof(AppSettings)).Bind(settings));

            // Add Serilog Logging
            var logger = SemanticDocIngestorLoggingExtensions.AddSerilogLogging(configuration);
            services.AddSingleton(logger);
            services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(dispose: true));

            // Register Ollama and Docs Service Factory
            services
                .RegisterDocumentProcessorFactory()
                .RegisterOllamaFactory(configuration);

            // Add APM
            if (useApm == true)
                services.AddApm();

            // Add Persistence
            services
                .AddCache()
                .AddVectorStore(configuration)
                .AddElasticSearch(configuration);

            // Add Middlewares
            services.AddMiddlewares(configuration);

            return services;
        }

        public static IApplicationBuilder UseInfrastructure(this IApplicationBuilder app, IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            // Use Serilog Logging
            app.UseLogging(configuration, loggerFactory);
            
            // Use Middleware
            app.UseMiddlewares();

            // Use Ollama client factory
            app.UseOllamaClient(configuration);

            return app;
        }
    }
}
