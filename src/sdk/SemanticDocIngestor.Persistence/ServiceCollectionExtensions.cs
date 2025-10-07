using SemanticDocIngestor.Domain.Abstracts.Persistence;
using SemanticDocIngestor.Domain.Abstracts.Settings;
using SemanticDocIngestor.Domain.Contracts;
using SemanticDocIngestor.Persistence.Cache.Memory;
using SemanticDocIngestor.Persistence.NoSQL.MongoDB.Repository;
using SemanticDocIngestor.Persistence.NoSQL.MongoDB.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace SemanticDocIngestor.Persistence
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSemanticDocIngestorMemoryCache(this IServiceCollection services)
        {
            services.AddMemoryCache(); // ensures IMemoryCache is available
            services.AddScoped<ICacheProvider, MemoryCacheProvider>();
            return services;
        }

        public static IServiceCollection AddSemanticDocIngestorMongoDb(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<MongoDbSettings>(settings => configuration.GetSection(nameof(MongoDbSettings)).Bind(settings));

            services.AddSingleton<IMongoDbSettings>(serviceProvider =>
                serviceProvider.GetRequiredService<IOptions<MongoDbSettings>>().Value);

            services.AddScoped(typeof(IMongoRepository<>), typeof(MongoRepository<>));

            return services;
        }
    }
}
