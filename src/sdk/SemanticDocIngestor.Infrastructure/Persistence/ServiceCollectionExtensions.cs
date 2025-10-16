using Elastic.Clients.Elasticsearch;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Qdrant.Client;
using SemanticDocIngestor.Domain.Abstractions.Persistence;
using SemanticDocIngestor.Domain.Abstractions.Settings;
using SemanticDocIngestor.Domain.Constants;
using SemanticDocIngestor.Domain.Options;
using SemanticDocIngestor.Infrastructure.Persistence.ElasticSearch;
using SemanticDocIngestor.Infrastructure.Persistence.VectorDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SemanticDocIngestor.Infrastructure.Persistence
{
    internal static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCache(this IServiceCollection services)
        {
            services.AddHybridCache();
            return services;
        }

        public static IServiceCollection AddVectorStore(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString(ConstantKeys.ConnectionStrings.Qdrant);
            var endpoint = connectionString?.Split(";")[0].Replace("Endpoint=", "");
            var key = connectionString?.Split(";")[1].Replace("Key=", "");
            var client = new QdrantClient(
                new Uri(endpoint ?? throw new InvalidOperationException("Qdrant endpoint cannot be null.")), key);
            services.AddSingleton(client);

            services.AddSingleton<IVectorStore, VectorStore>();

            return services;
        }

        public static IServiceCollection AddElasticSearch(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString(ConstantKeys.ConnectionStrings.ElasticSearch);

            services.AddSingleton(new ElasticsearchClient(new Uri(connectionString ??
                throw new ArgumentNullException("Elastic search connection string is not configured!"))));

            services.AddSingleton<IElasticStore, ElasticStore>();

            return services;
        }
    }
}
