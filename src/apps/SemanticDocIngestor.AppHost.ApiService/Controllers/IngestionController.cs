using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Nodes;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using SemanticDocIngestor.Domain.Abstractions.Persistence;
using SemanticDocIngestor.Domain.Abstractions.Services;
using SemanticDocIngestor.Domain.DTOs;
using SemanticDocIngestor.Domain.Entities.Ingestion;
using System.ComponentModel.DataAnnotations;

namespace SemanticDocIngestor.AppHost.ApiService.Controllers
{
    /// <summary>
    /// API controller for document ingestion, search, and RAG operations.
    /// Provides endpoints for managing document lifecycle and performing hybrid search with AI-powered answers.
    /// </summary>
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

    /// <summary>
        /// Ingests all supported documents from a specified folder recursively.
    /// </summary>
        /// <param name="folderPath">Relative path to the folder within wwwroot (e.g., "uploads/documents").</param>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <returns>HTTP 201 Created on successful ingestion.</returns>
        /// <response code="201">Documents were successfully ingested.</response>
        /// <response code="404">The specified folder was not found.</response>
        /// <response code="400">Invalid folder path provided.</response>
 [HttpPost("ingest-folder")]
   [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> IngestAsync([Required] string folderPath = "", CancellationToken cancellationToken = default)
{
            await _documentIngestor.IngestFolderAsync(Path.Combine(_env.WebRootPath, folderPath), cancellationToken);
        return Created();
        }

        /// <summary>
        /// Ingests specific files from a list of file paths.
        /// Supports local files, OneDrive URIs, and Google Drive URIs.
        /// </summary>
   /// <param name="filesPath">List of relative file paths within wwwroot or cloud URIs (e.g., "uploads/doc.pdf", "onedrive://...", "gdrive://...").</param>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <returns>HTTP 201 Created on successful ingestion.</returns>
        /// <response code="201">Files were successfully ingested.</response>
        /// <response code="400">Invalid file paths provided or unsupported file types.</response>
        /// <remarks>
      /// Supported cloud URIs:
        /// - OneDrive: "onedrive://{driveId}/{itemId}" or share links
  /// - Google Drive: "gdrive://{fileId}" or drive.google.com URLs
    /// </remarks>
      [HttpPost("ingest-files")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> IngestFileAsync([Required] List<string> filesPath, CancellationToken cancellationToken = default)
        {
    await _documentIngestor.IngestDocumentsAsync(filesPath.Select(file => Path.Combine(_env.WebRootPath, file)), cancellationToken: cancellationToken);
    return Created();
        }

    /// <summary>
        /// Retrieves a list of all documents that have been ingested into the system.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <returns>List of ingested documents with metadata including file names, sources, and timestamps.</returns>
        /// <response code="200">Successfully retrieved the list of ingested documents.</response>
        [HttpGet("ingested-files")]
        [ProducesResponseType(typeof(List<DocumentRepoItemDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetIngestedFilesAsync(CancellationToken cancellationToken = default)
        {
        var ingestedFiles = await _documentIngestor.ListIngestedDocumentsAsync(cancellationToken);
        return Ok(ingestedFiles);
        }

        /// <summary>
        /// Deletes all ingested documents from both vector and keyword stores.
        /// This operation is irreversible and removes all indexed data.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <returns>HTTP 200 OK on successful deletion.</returns>
     /// <response code="200">All documents were successfully deleted.</response>
/// <response code="500">An error occurred during deletion.</response>
 [HttpDelete("flush-db")]
        [ProducesResponseType(StatusCodes.Status200OK)]
 [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> FlushAsync(CancellationToken cancellationToken = default)
        {
   await _documentIngestor.FlushAsync(cancellationToken);
            return Ok();
}

  /// <summary>
        /// Gets the current ingestion progress including the number of documents processed.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
   /// <returns>Current ingestion progress with completed and total counts.</returns>
   /// <response code="200">Successfully retrieved ingestion progress.</response>
 [HttpGet("progress")]
        [ProducesResponseType(typeof(IngestionProgress), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetProgressAsync(CancellationToken cancellationToken = default)
   {
       var progress = await _documentIngestor.GetProgressAsync(cancellationToken);
            return Ok(progress);
        }

 /// <summary>
        /// Performs hybrid search across ingested documents and generates an AI-powered answer using RAG.
 /// Combines vector similarity search and keyword search for optimal results.
        /// </summary>
        /// <param name="search">The search query or question (minimum 5 characters).</param>
      /// <param name="limit">Maximum number of document chunks to use as context for the answer (default: 5).</param>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
     /// <returns>Generated answer with references to source documents.</returns>
        /// <response code="200">Successfully generated answer with source references.</response>
        /// <response code="400">Search query is too short or invalid.</response>
      /// <remarks>
        /// The search query must be at least 5 characters long.
        /// The answer is generated using Ollama LLM based on the most relevant document chunks.
     /// </remarks>
  [HttpGet("search")]
        [ProducesResponseType(typeof(SearchAndGetRagResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SearchAsync(
         [MinLength(5, ErrorMessage = "Search term can not be less than 5 chars!")]
         [Required] string search = "", ulong limit = 5, CancellationToken cancellationToken = default)
        {
       var response = await _documentIngestor.SearchAndGetRagResponseAsync(search, limit, cancellationToken);
    return Ok(response);
        }
    }
}
