using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SemanticDocIngestor.Domain.Options;

namespace SemanticDocIngestor.Infrastructure.Middlewares
{
    internal static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMiddlewares(this IServiceCollection services, IConfiguration configuration)
        {
            ResiliencyMiddlewareOptions options = new();
            services.Configure<ResiliencyMiddlewareOptions>(options => configuration.GetSection(nameof(ResiliencyMiddlewareOptions)).Bind(options));
            configuration.GetSection(nameof(ResiliencyMiddlewareOptions)).Bind(options);

            return services;
        }

        public static IApplicationBuilder UseMiddlewares(this IApplicationBuilder app)
        {
            app.UseSemanticDocIngestorRequestLogging();     // logs all incoming requests
            app.UseSemanticDocIngestorExceptionHandler();   // catch and serialize any errors
            app.UseSemanticDocIngestorResiliency();         // retry, timeout, circuit breaker

            return app;
        }
    }
}
