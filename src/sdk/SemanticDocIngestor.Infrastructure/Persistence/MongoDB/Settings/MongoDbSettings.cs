using SemanticDocIngestor.Domain.Abstractions.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SemanticDocIngestor.Infrastructure.Persistence.MongoDB.Settings
{
    public class MongoDbSettings : IMongoDbSettings
    {
        public string DatabaseName { get; set; }
    }
}
