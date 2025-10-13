using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace SemanticDocIngestor.Application
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration, bool useApm = false)
        {
            return services;
        }

        public static IApplicationBuilder UseApplication(this IApplicationBuilder app, IConfiguration Configuration, ILoggerFactory loggerFactory)
        {
            return app;
        }
    }
}
