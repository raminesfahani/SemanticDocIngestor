using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SemanticDocIngestor.Domain.Entities.Ingestion
{
    public class IngestionProgress
    {
        public string FilePath { get; set; } = string.Empty;
        public int Completed { get; set; }
        public int Total { get; set; }
    }
}
