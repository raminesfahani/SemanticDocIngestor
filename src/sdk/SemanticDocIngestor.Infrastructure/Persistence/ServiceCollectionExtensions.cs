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
            
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new InvalidOperationException("Qdrant connection string cannot be null or empty.");

            string endpoint;
            string? key = null;

            // Check if connection string contains semicolon (formatted: Endpoint=...;Key=...)
            if (connectionString.Contains(';'))
            {
                var parts = connectionString.Split(";");
                endpoint = parts[0].Replace("Endpoint=", "").Trim();
    
                if (parts.Length > 1 && parts[1].Contains("Key="))
                {
                    key = parts[1].Replace("Key=", "").Trim();
                }
            }
            else
            {
                // Simple URL format
                endpoint = connectionString.Trim();
            }

            var client = new QdrantClient(new Uri(endpoint), key);
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
