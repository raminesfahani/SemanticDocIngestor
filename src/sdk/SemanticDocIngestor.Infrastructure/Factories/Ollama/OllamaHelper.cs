using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SemanticDocIngestor.Infrastructure.Factories.Ollama
{
    public static class OllamaHelper
    {
        public static string GetOllamaEndpointUri(string connectionString) => connectionString?.Replace("Endpoint=", "") + "/api";
    }
}
