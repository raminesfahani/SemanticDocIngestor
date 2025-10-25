using SemanticDocIngestor.AppHost.ApiService.Models;
using Microsoft.AspNetCore.Mvc;

namespace SemanticDocIngestor.AppHost.ApiService.Controllers
{
    /// <summary>
    /// API controller for managing file uploads and retrieving uploaded files.
    /// Handles file upload operations to the server's wwwroot/uploads directory.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AssetsController(IWebHostEnvironment env) : ControllerBase
    {
        private readonly IWebHostEnvironment _env = env;

        /// <summary>
        /// Uploads a file to the server's uploads directory with a random filename.
        /// The original file extension is preserved.
        /// </summary>
        /// <param name="file">The file to upload.</param>
        /// <returns>Upload response containing the new filename, file size, and relative path.</returns>
        /// <response code="200">File was successfully uploaded.</response>
        /// <response code="400">No file was selected or file is empty.</response>
        /// <remarks>
        /// The file is saved with a random GUID-based filename to prevent conflicts.
        /// Example response:
        /// <code>
        /// {
        ///   "fileName": "a1b2c3d4-e5f6-7890-abcd-ef1234567890.pdf",
        ///   "length": 102400,
        ///   "filePath": "uploads/a1b2c3d4-e5f6-7890-abcd-ef1234567890.pdf"
        /// }
        /// </code>
        /// </remarks>
        [HttpPost]
        [ProducesResponseType(typeof(FileUploadResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("File not selected");

            var uploadsPath = Path.Combine(_env.WebRootPath, "uploads");
            Directory.CreateDirectory(uploadsPath);

            // Get the file extension (including the dot)
            var extension = Path.GetExtension(file.FileName);

            // Generate a random filename
            var randomFileName = $"{Guid.NewGuid()}{extension}";

            var filePath = Path.Combine(uploadsPath, randomFileName);

            await using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            return Ok(new FileUploadResponse()
            {
                FileName = randomFileName,
                Length = file.Length,
                FilePath = Path.Combine("uploads", randomFileName)
            });
        }

        /// <summary>
        /// Retrieves a list of all files in the uploads directory.
        /// </summary>
        /// <returns>Array of filenames in the uploads directory.</returns>
        /// <response code="200">Successfully retrieved the list of uploaded files.</response>
        /// <remarks>
        /// Returns an empty array if the uploads directory does not exist or is empty.
        /// </remarks>
        [HttpGet]
        [ProducesResponseType(typeof(string[]), StatusCodes.Status200OK)]
        public IActionResult GetUploadedFiles()
        {
            var uploadsPath = Path.Combine(_env.WebRootPath, "uploads");

            if (!Directory.Exists(uploadsPath))
                return Ok(Array.Empty<string>());

            var files = Directory.GetFiles(uploadsPath)
                .Select(Path.GetFileName)
                .ToArray();

            return Ok(files);
        }
    }
}