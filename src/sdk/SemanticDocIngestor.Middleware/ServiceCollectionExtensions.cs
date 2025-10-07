using SemanticDocIngestor.Domain.Contracts;
using SemanticDocIngestor.Domain.Options;
using SemanticDocIngestor.Middleware.Middlewares;
using SemanticDocIngestor.Middleware.Serilog;
using SemanticDocIngestor.Persistence;
using SemanticDocIngestor.Persistence.NoSQL.MongoDB;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace SemanticDocIngestor.Middleware
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSemanticDocIngestorMiddleware(this IServiceCollection services, IConfiguration configuration, bool useApm = false)
        {
            ResiliencyMiddlewareOptions options = new();
            services.Configure<ResiliencyMiddlewareOptions>(configuration.GetSection(nameof(ResiliencyMiddlewareOptions)));
            configuration.GetSection(nameof(ResiliencyMiddlewareOptions)).Bind(options);

            services.AddSemanticDocIngestorMemoryCache();
            services.AddSemanticDocIngestorMongoDb(configuration);

            if (useApm == true)
                services.AddApm();

            return services;
        }

        public static IApplicationBuilder UseSemanticDocIngestorMiddleware(this IApplicationBuilder app, IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            app.UseLogging(configuration, loggerFactory);
            app.UseSemanticDocIngestorRequestLogging();     // logs all incoming requests
            app.UseSemanticDocIngestorExceptionHandler();   // catch and serialize any errors
            app.UseSemanticDocIngestorResiliency();         // retry, timeout, circuit breaker

            return app;
        }
    }
}
