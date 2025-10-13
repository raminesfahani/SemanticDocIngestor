using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ollama;
using SemanticDocIngestor.Domain.Abstractions.Factories;
using SemanticDocIngestor.Domain.Abstractions.Services;
using SemanticDocIngestor.Domain.Constants;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SemanticDocIngestor.Infrastructure.Factories.Ollama
{
    internal static class ServiceCollectionExtensions
    {
        public static IServiceCollection RegisterOllamaFactory(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IOllamaServiceFactory, OllamaServiceFactory>()
                    .AddSingleton<IRagService, RagService>();

            // Register singleton Ollama API client with base endpoint from config
            services.AddSingleton<IOllamaApiClient>(factory =>
            {
                var uri = OllamaHelper.GetOllamaEndpointUri(configuration.GetConnectionString(ConstantKeys.ConnectionStrings.Ollama) ?? throw new InvalidOperationException("Ollama endpoint URL is not configured!"));
                return new OllamaApiClient(baseUri: new Uri(uri));
            });

            return services;
        }
    }
}
