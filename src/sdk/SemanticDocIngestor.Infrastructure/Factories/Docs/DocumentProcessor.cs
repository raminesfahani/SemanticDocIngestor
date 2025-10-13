using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using SemanticDocIngestor.Domain.Abstractions.Factories;
using SemanticDocIngestor.Domain.Abstractions.Services;
using SemanticDocIngestor.Domain.Entities.Ingestion;
using SemanticDocIngestor.Domain.Exceptions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Tesseract;
using UglyToad.PdfPig;
using Text = DocumentFormat.OpenXml.Wordprocessing.Text;

namespace SemanticDocIngestor.Infrastructure.Factories.Docs;

public class DocumentProcessor(IOllamaServiceFactory ollamaServiceFactory, IRagService ragService) : IDocumentProcessor
{
    private readonly IOllamaServiceFactory _ollamaServiceFactory = ollamaServiceFactory;
    private readonly IRagService _ragService = ragService;

    public List<string> SupportedFileExtensions =>
    [
        ".pdf",
        ".docx",
        ".xlsx", ".xls",
        ".jpg", ".jpeg", ".png", ".tiff", ".bmp",
        ".txt",
        ".rtf",
        ".html", ".htm",
        ".json",
        ".md"
    ];

    public async Task<List<DocumentChunk>> ProcessDocument(string filePath, int maxChunkSize = 500, CancellationToken cancellationToken = default)
    {
        string extension = Path.GetExtension(filePath).ToLower();

        return extension switch
        {
            ".pdf" => await ProcessPdf(filePath, maxChunkSize),
            ".docx" => await ProcessDocx(filePath, maxChunkSize),
            ".xlsx" or ".xls" => await ProcessExcel(filePath, maxChunkSize),
            ".jpg" or ".jpeg" or ".png" or ".tiff" or ".bmp" => await ProcessImage(filePath, maxChunkSize),
            ".txt" => await ProcessTextFile(filePath, maxChunkSize),
            ".rtf" => await ProcessRtf(filePath, maxChunkSize),
            ".html" or ".htm" => await ProcessHtml(filePath, maxChunkSize),
            ".json" or ".md" => await ProcessDefault(filePath, maxChunkSize),
            _ => throw new UnsupportedFileTypeException($"Format {extension} not supported")
        };
    }

    private async Task<float[]> GetEmbeddingsAsync(string content)
    {
        if (string.IsNullOrEmpty(content)) return [];

        var embedding = await _ollamaServiceFactory.GetEmbeddingAsync(content);
        return [.. embedding];
    }

    private async Task<List<DocumentChunk>> WrapChunks(List<string> contentChunks, string filePath, string fileType, Action<IngestionMetadata>? enrich = null)
    {
        var result = new List<DocumentChunk>();
        int index = 0;
        foreach (var chunk in contentChunks)
        {
            var metadata = new IngestionMetadata
            {
                FileName = Path.GetFileName(filePath),
                FileType = fileType,
                FilePath = filePath
            };

            enrich?.Invoke(metadata);

            result.Add(new DocumentChunk
            {
                Content = chunk,
                Embedding = await GetEmbeddingsAsync(chunk),
                Index = index++,
                Metadata = metadata
            });
        }

        return result;
    }

    private async Task<List<DocumentChunk>> ProcessDefault(string filePath, int maxChunkSize)
    {
        var content = File.ReadAllText(filePath);
        var chunks = await _ragService.GetDocumentChunksAsync(content, default);
        return await WrapChunks(chunks, filePath, Path.GetExtension(filePath));
    }

    private async Task<List<DocumentChunk>> ProcessPdf(string filePath, int maxChunkSize)
    {
        var chunks = new List<DocumentChunk>();
        int globalIndex = 0;

        using var document = PdfDocument.Open(filePath);
        int pageNum = 1;

        foreach (var page in document.GetPages())
        {
            var text = string.Join(" ", page.GetWords().Select(w => w.Text));
            var pageChunks = await _ragService.GetDocumentChunksAsync(text, default);

            foreach (var chunk in pageChunks)
            {
                chunks.Add(new DocumentChunk
                {
                    Content = chunk,
                    Index = globalIndex++,
                    Embedding = await GetEmbeddingsAsync(chunk),
                    Metadata = new IngestionMetadata
                    {
                        FileName = Path.GetFileName(filePath),
                        FileType = ".pdf",
                        FilePath = filePath,
                        PageNumber = pageNum.ToString(),
                    }
                });
            }

            pageNum++;
        }

        return chunks;
    }

    private async Task<List<DocumentChunk>> ProcessDocx(string filePath, int maxChunkSize)
    {
        var sb = new StringBuilder();

        using var doc = WordprocessingDocument.Open(filePath, false);
        var body = doc.MainDocumentPart?.Document.Body;

        foreach (var para in body?.Elements<Paragraph>() ?? [])
        {
            var text = string.Join(" ", para.Descendants<Text>().Select(t => t.Text));
            sb.AppendLine(text);
        }

        var chunks = await _ragService.GetDocumentChunksAsync(sb.ToString(), default);

        return await WrapChunks(chunks, filePath, ".docx");
    }

    private async Task<List<DocumentChunk>> ProcessExcel(string filePath, int maxChunkSize)
    {
        var chunks = new List<DocumentChunk>();
        using var document = SpreadsheetDocument.Open(filePath, false);
        var workbookPart = document.WorkbookPart;
        var sheets = workbookPart!.Workbook.Sheets?.Cast<Sheet>();
        var sharedStrings = workbookPart.SharedStringTablePart?.SharedStringTable;

        int globalIndex = 0;

        foreach (var sheet in sheets ?? [])
        {
            var worksheetPart = (WorksheetPart)workbookPart.GetPartById(sheet.Id!);
            var sheetData = worksheetPart.Worksheet.Elements<SheetData>().FirstOrDefault();
            if (sheetData == null) continue;

            int rowIndex = 0;
            foreach (var row in sheetData.Elements<Row>())
            {
                var rowText = new StringBuilder();
                foreach (var cell in row.Elements<Cell>())
                {
                    var value = cell.CellValue?.InnerText ?? "";
                    if (cell.DataType?.Value == CellValues.SharedString && int.TryParse(value, out var id) && sharedStrings != null)
                    {
                        value = sharedStrings.ElementAt(id).InnerText;
                    }

                    rowText.Append(value).Append(' ');
                }

                var chunkContent = rowText.ToString().Trim();
                if (!string.IsNullOrEmpty(chunkContent))
                {
                    chunks.Add(new DocumentChunk
                    {
                        Content = chunkContent,
                        Index = globalIndex++,
                        Embedding = await GetEmbeddingsAsync(chunkContent),
                        Metadata = new IngestionMetadata
                        {
                            FileName = Path.GetFileName(filePath),
                            FileType = ".xlsx",
                            FilePath = filePath,
                            SheetName = sheet.Name,
                            RowIndex = rowIndex++
                        }
                    });
                }
            }
        }

        return chunks;
    }

    private async Task<List<DocumentChunk>> ProcessTextFile(string filePath, int maxChunkSize)
    {
        var content = File.ReadAllText(filePath);
        var chunks = await _ragService.GetDocumentChunksAsync(content, default);
        return await WrapChunks(chunks, filePath, ".txt");
    }

    private async Task<List<DocumentChunk>> ProcessRtf(string filePath, int maxChunkSize)
    {
        var content = File.ReadAllText(filePath);
        content = Regex.Replace(content, @"\[a-zA-Z]+\d*", " ");
        content = Regex.Replace(content, @"[{\}]", "");
        content = Regex.Replace(content, @"\s+", " ");

        var chunks = await _ragService.GetDocumentChunksAsync(content.Trim(), default);
        return await WrapChunks(chunks, filePath, ".rtf");
    }

    private async Task<List<DocumentChunk>> ProcessHtml(string filePath, int maxChunkSize)
    {
        var html = File.ReadAllText(filePath);
        var plain = Regex.Replace(html, @"<[^>]+>", " ");
        var chunks = await _ragService.GetDocumentChunksAsync(plain.Trim(), default);
        return await WrapChunks(chunks, filePath, ".html");
    }

    private async Task<List<DocumentChunk>> ProcessImage(string filePath, int maxChunkSize)
    {
        var chunks = new List<DocumentChunk>();
        try
        {
            using var engine = new TesseractEngine("./tessdata", "eng", EngineMode.Default);
            using var img = Pix.LoadFromFile(filePath);
            using var page = engine.Process(img);

            string text = page.GetText();
            var imageChunks = await _ragService.GetDocumentChunksAsync(text, default);

            int index = 0;
            foreach (var chunk in imageChunks)
            {
                chunks.Add(new DocumentChunk
                {
                    Content = chunk,
                    Index = index++,
                    Embedding = await GetEmbeddingsAsync(chunk),
                    Metadata = new IngestionMetadata
                    {
                        FileName = Path.GetFileName(filePath),
                        FileType = Path.GetExtension(filePath),
                        FilePath = filePath,
                    }
                });
            }
        }
        catch (Exception ex)
        {
            chunks.Add(new DocumentChunk
            {
                Content = $"[OCR_ERROR: {ex.Message}]",
                Index = 0,
                Embedding = [],
                Metadata = new IngestionMetadata
                {
                    FileName = Path.GetFileName(filePath),
                    FileType = Path.GetExtension(filePath),
                    FilePath = filePath,
                }
            });
        }

        return chunks;
    }
}
