using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Nodes;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using SemanticDocIngestor.Domain.Abstractions.Persistence;
using SemanticDocIngestor.Domain.Abstractions.Services;
using SemanticDocIngestor.Domain.Entities.Ingestion;
using System.ComponentModel.DataAnnotations;

namespace SemanticDocIngestor.AppHost.ApiService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class IngestionController(ILogger<IngestionController> logger,
                                     IDocumentIngestorService documentIngestor,
                                    IWebHostEnvironment env,
                                     IRagService ragService) : ControllerBase
    {
        private readonly ILogger<IngestionController> _logger = logger;
        private readonly IDocumentIngestorService _documentIngestor = documentIngestor;
        private readonly IWebHostEnvironment _env = env;
        private readonly IRagService _ragService = ragService;

        [HttpPost("ingest-folder")]
        public async Task<IActionResult> IngestAsync([Required] string folderPath = "", CancellationToken cancellationToken = default)
        {
            await _documentIngestor.IngestFolderAsync(Path.Combine(_env.WebRootPath, folderPath), cancellationToken);
            return Created();
        }

        [HttpPost("ingest-files")]
        public async Task<IActionResult> IngestFileAsync([Required] List<string> filesPath, CancellationToken cancellationToken = default)
        {
            await _documentIngestor.IngestDocumentsAsync(filesPath.Select(file => Path.Combine(_env.WebRootPath, file)), cancellationToken: cancellationToken);
            return Created();
        }

        [HttpDelete("flush-db")]
        public async Task<IActionResult> FlushAsync(CancellationToken cancellationToken = default)
        {
            await _documentIngestor.FlushAsync(cancellationToken);
            return Ok();
        }

        [HttpGet("progress")]
        public async Task<IActionResult> GetProgressAsync(CancellationToken cancellationToken = default)
        {
            var progress = await _documentIngestor.GetProgressAsync(cancellationToken);
            return Ok(progress);
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchAsync(
            [MinLength(5, ErrorMessage = "Search term can not be less than 5 chars!")]
            [Required] string search = "", CancellationToken cancellationToken = default)
        {
            var contextChunks = await _documentIngestor.SearchDocumentsAsync(search, limit: 5, cancellationToken: cancellationToken);
            var ragResponse = await _ragService.GetAnswerAsync(search, contextChunks, cancellationToken: cancellationToken);

            return Ok(new
            {
                response = ragResponse,
                references = contextChunks.GroupBy(c => c.Metadata.FilePath).Select(g =>
                new
                {
                    filePath = g.Key,
                    excerpts = g.Select(c => new
                    {
                        content = c.Content,
                        pageNumber = c.Metadata.PageNumber
                    }).ToList()
                }).ToList()
            });
        }
    }
}
