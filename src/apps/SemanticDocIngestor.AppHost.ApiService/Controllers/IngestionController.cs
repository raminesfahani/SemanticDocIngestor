using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Nodes;
using Microsoft.AspNetCore.Mvc;
using SemanticDocIngestor.Domain.Abstractions.Services;
using SemanticDocIngestor.Domain.Entities.Ingestion;
using System.ComponentModel.DataAnnotations;

namespace SemanticDocIngestor.AppHost.ApiService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class IngestionController(ILogger<IngestionController> logger,
                                     IDocumentIngestorService documentIngestor,
                                     IRagService ragService) : ControllerBase
    {
        private readonly ILogger<IngestionController> _logger = logger;
        private readonly IDocumentIngestorService _documentIngestor = documentIngestor;
        private readonly IRagService _ragService = ragService;

        [HttpPost("ingest")]
        public async Task<IActionResult> IngestAsync(CancellationToken cancellationToken = default)
        {
            await _documentIngestor.IngestDocumentsAsync(
            [
                "C:\\Users\\resfa\\Downloads\\example-revenue-trend-report.pdf"
            ], default, cancellationToken);

            return Ok();
        }

        [HttpDelete("flush")]
        public async Task<IActionResult> FlushAsync(CancellationToken cancellationToken = default)
        {
            await _documentIngestor.FlushAsync(cancellationToken);

            return Ok();
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchAsync(
            [MinLength(3, ErrorMessage = "Search term can not be less than 3 chars!")]
            [Required] string search = "", CancellationToken cancellationToken = default)
        {
            var contextChunks = await _documentIngestor.SearchDocumentsAsync(search, limit: 10, cancellationToken: cancellationToken);
            var ragResponse = await _ragService.GetAnswerAsync(search, contextChunks, cancellationToken: cancellationToken);

            return Ok(ragResponse);
        }
    }
}
