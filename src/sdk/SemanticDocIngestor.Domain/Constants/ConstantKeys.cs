using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SemanticDocIngestor.Domain.Constants
{
    public static class ConstantKeys
    {
        public static class ConnectionStrings
        {
            public const string Ollama = "ollama";
            public const string Qdrant = "qdrant";
            public const string ElasticSearch = "elasticsearch";
        }
    }
}
