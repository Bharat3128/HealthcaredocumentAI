using System.Text.Json;
using Healthcare.Api.Data;
using Healthcare.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Healthcare.Api.Services;

public class DocumentService
{
    
    private readonly Healthcare.Api.Storage.IFileStorageService _fileStorageService;
private readonly IWebHostEnvironment _environment;
    private readonly HealthcareDbContext _dbContext;
    private readonly IDocumentTextExtractor _documentTextExtractor;
    private readonly DocumentClassificationService _classificationService;
    private readonly StructuredExtractionService _structuredExtractionService;
    private readonly DocumentValidationService _validationService;
    private readonly DocumentChunkingService _chunkingService;
    private readonly EmbeddingService _embeddingService;

    public DocumentService(
        IWebHostEnvironment environment,
        HealthcareDbContext dbContext,
        IDocumentTextExtractor documentTextExtractor,
        DocumentClassificationService classificationService,
        StructuredExtractionService structuredExtractionService,
        DocumentValidationService validationService,
        DocumentChunkingService chunkingService,
        EmbeddingService embeddingService,
        Healthcare.Api.Storage.IFileStorageService fileStorageService)
    {
        
        _fileStorageService = fileStorageService;
_environment = environment;
        _dbContext = dbContext;
        _documentTextExtractor = documentTextExtractor;
        _classificationService = classificationService;
        _structuredExtractionService = structuredExtractionService;
        _validationService = validationService;
        _chunkingService = chunkingService;
        _embeddingService = embeddingService;
    }

    public async Task<Document> SaveDocumentAsync(
        IFormFile file,
        CancellationToken cancellationToken = default)
    {
        const long maxFileSize = 25 * 1024 * 1024;

        if (file == null || file.Length == 0)
        {
            throw new InvalidDataException(
                "A PDF file is required.");
        }

        if (file.Length > maxFileSize)
        {
            throw new InvalidDataException(
                "PDF file size cannot exceed 25 MB.");
        }

        var extension =
            Path.GetExtension(file.FileName);

        if (!string.Equals(
                extension,
                ".pdf",
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidDataException(
                "Only PDF files are allowed.");
        }

        if (!string.Equals(
                file.ContentType,
                "application/pdf",
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidDataException(
                "Invalid PDF content type.");
        }

        await using (var inputStream =
            file.OpenReadStream())
        {
            var signature = new byte[4];

            var bytesRead =
                await inputStream.ReadAsync(
                    signature.AsMemory(0, 4),
                    cancellationToken);

            var validPdf =
                bytesRead == 4 &&
                signature[0] == 0x25 &&
                signature[1] == 0x50 &&
                signature[2] == 0x44 &&
                signature[3] == 0x46;

            if (!validPdf)
            {
                throw new InvalidDataException(
                    "The uploaded file is not a valid PDF.");
            }
        }

        var documentId = Guid.NewGuid();

        var originalFileName =
            Path.GetFileName(file.FileName);

        await using var uploadStream =
            file.OpenReadStream();

        await _fileStorageService.SaveAsync(
            documentId,
            uploadStream,
            cancellationToken);



        var document = new Document
        {
            Id = documentId,
            FileName = originalFileName,
            Status = "Uploaded",
            UploadedAt = DateTime.UtcNow
        };

        _dbContext.Documents.Add(document);

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return document;
    }

    public IEnumerable<Document> GetDocuments()
    {
        return _dbContext.Documents
            .AsNoTracking()
            .OrderByDescending(document => document.UploadedAt)
            .ToList();
    }

    public async Task<List<DocumentChunk>> GetChunksAsync(Guid documentId)
    {
        return await _dbContext.DocumentChunks
            .AsNoTracking()
            .Where(chunk => chunk.DocumentId == documentId)
            .OrderBy(chunk => chunk.ChunkIndex)
            .ToListAsync();
    }

    public async Task<Document?> ProcessDocumentAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var document = await _dbContext.Documents
            .FirstOrDefaultAsync(document => document.Id == id);

        if (document == null)
        {
            return null;
        }

        document.Status = "Processing";

        await _dbContext.SaveChangesAsync();

        await using var storedFile =
            await _fileStorageService.OpenReadAsync(
                document.Id,
                cancellationToken);

        document.ExtractedText =
            await _documentTextExtractor.ExtractTextAsync(
                storedFile);

        document.DocumentType =
            _classificationService.Classify(
                document.ExtractedText);

        document.StructuredDataJson =
            _structuredExtractionService.Extract(
                document.ExtractedText,
                document.DocumentType);

        document.ValidationStatus =
            _validationService.Validate(
                document.DocumentType,
                document.StructuredDataJson);

        await _dbContext.DocumentChunks
            .Where(chunk =>
                chunk.DocumentId == document.Id)
            .ExecuteDeleteAsync(cancellationToken);

        var chunks =
            _chunkingService.CreateChunks(
                document.Id,
                document.ExtractedText);

        foreach (var chunk in chunks)
        {
            var embedding =
                await _embeddingService.GenerateEmbeddingAsync(
                    chunk.Content,
                    cancellationToken);

            chunk.EmbeddingJson =
                JsonSerializer.Serialize(embedding);
        }

        if (chunks.Count > 0)
        {
            _dbContext.DocumentChunks.AddRange(chunks);
        }

        document.Status = "Completed";
        document.ProcessingCompletedAt = DateTime.UtcNow;
        document.ProcessingError = null;

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return document;
    }
}








