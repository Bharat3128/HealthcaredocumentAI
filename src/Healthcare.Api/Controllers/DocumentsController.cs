using Healthcare.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Healthcare.Api.Controllers;

public class AskRequest
{
    public string Question { get; set; } = string.Empty;
}

[ApiController]
[Route("api/[controller]")]
public class DocumentsController : ControllerBase
{
    private readonly DocumentService _documentService;
    private readonly DocumentRetrievalService _retrievalService;
    private readonly EmbeddingBackfillService _embeddingBackfillService;
    private readonly RagService _ragService;

    public DocumentsController(
        DocumentService documentService,
        DocumentRetrievalService retrievalService,
        EmbeddingBackfillService embeddingBackfillService,
        RagService ragService)
    {
        _documentService = documentService;
        _retrievalService = retrievalService;
        _embeddingBackfillService = embeddingBackfillService;
        _ragService = ragService;
    }

    [HttpGet]
    public IActionResult GetDocuments()
    {
        return Ok(_documentService.GetDocuments());
    }

    [HttpGet("{id}/chunks")]
    public async Task<IActionResult> GetChunks(Guid id)
    {
        return Ok(
            await _documentService.GetChunksAsync(id));
    }

    [HttpGet("{id}/search")]
    public async Task<IActionResult> Search(
        Guid id,
        [FromQuery] string query)
    {
        return Ok(
            await _retrievalService.SearchAsync(
                id,
                query));
    }

    [HttpGet("{id}/semantic-search")]
    public async Task<IActionResult> SemanticSearch(
        Guid id,
        [FromQuery] string query)
    {
        return Ok(
            await _retrievalService.SemanticSearchAsync(
                id,
                query));
    }

    [HttpPost("{id}/embeddings")]
    public async Task<IActionResult> GenerateEmbeddings(Guid id)
    {
        var count =
            await _embeddingBackfillService
                .GenerateEmbeddingsAsync(id);

        return Ok(new
        {
            documentId = id,
            embeddingsGenerated = count
        });
    }

    [HttpPost("{id}/ask")]
    public async Task<IActionResult> Ask(
        Guid id,
        [FromBody] AskRequest? request,
        CancellationToken cancellationToken)
    {
        if (request == null ||
            string.IsNullOrWhiteSpace(request.Question))
        {
            return BadRequest(new
            {
                message = "Question is required."
            });
        }

        var result =
            await _ragService.AskAsync(
                id,
                request.Question,
                cancellationToken);

        return Ok(result);
    }

    [HttpPost("upload")]
    public async Task<IActionResult> UploadDocument(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new
            {
                message = "Please select a file."
            });
        }

        if (!file.FileName.EndsWith(
                ".pdf",
                StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new
            {
                message = "Only PDF files are allowed."
            });
        }

        return Ok(
            await _documentService.SaveDocumentAsync(file));
    }
    [HttpPost("{id}/process")]
    public async Task<IActionResult> ProcessDocument(
        Guid id,
        [FromServices] Healthcare.Api.Data.HealthcareDbContext dbContext,
        [FromServices] Healthcare.Api.Services.DocumentProcessingQueue queue,
        CancellationToken cancellationToken)
    {
        var document =
            await dbContext.Documents.FindAsync(
                new object[] { id },
                cancellationToken);

        if (document == null)
        {
            return NotFound(new
            {
                message = "Document not found."
            });
        }

        if (document.Status == "Queued" ||
            document.Status == "Processing")
        {
            return Conflict(new
            {
                documentId = id,
                status = document.Status,
                message = "Document is already being processed."
            });
        }

        document.Status = "Queued";

        await dbContext.SaveChangesAsync(
            cancellationToken);

        await queue.QueueAsync(
            id,
            cancellationToken);

        return Accepted(new
        {
            documentId = id,
            status = "Queued"
        });
    }
}


