using Microsoft.AspNetCore.Builder;
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

        public static IApplicationBuilder UseOllamaClient(this IApplicationBuilder app, IConfiguration configuration)
        {
            var appsettings = configuration.GetSection(nameof(Configurations.AppSettings)).Get<Configurations.AppSettings>() ?? throw new InvalidOperationException("AppSettings section is not configured properly!");
            var ollamaFactory = app.ApplicationServices.GetRequiredService<IOllamaServiceFactory>();

            // Ensure the embedding model is pulled and ready
            ollamaFactory.EnsureModelIsPulledAsync(appsettings.Ollama.EmbeddingModel).GetAwaiter().GetResult();
            ollamaFactory.EnsureModelIsPulledAsync(appsettings.Ollama.ChatModel).GetAwaiter().GetResult();

            return app;
        }
    }
}
