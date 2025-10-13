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
    public static class ServiceCollectionExtensions
    {
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

        public static IServiceCollection AddSemanticDocIngestorCore(this IServiceCollection services, IConfiguration configuration)
        {
            services.RegisterMappingProfiles()
                    .AddInfrastructure(configuration);

            services.AddSingleton<IDocumentIngestorService, DocumentIngestorService>();

            return services;
        }

        public static IApplicationBuilder UseSemanticDocIngestorCore(this IApplicationBuilder app, IConfiguration Configuration, ILoggerFactory loggerFactory)
        {
            app.UseInfrastructure(Configuration, loggerFactory);

            return app;
        }
    }
}
