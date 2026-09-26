using Healthcare.Api.Data;
using Healthcare.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace Healthcare.Api.Workers;

public class DocumentProcessingWorker : BackgroundService
{
    private readonly DocumentProcessingQueue _queue;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<DocumentProcessingWorker> _logger;

    public DocumentProcessingWorker(
        DocumentProcessingQueue queue,
        IServiceScopeFactory scopeFactory,
        ILogger<DocumentProcessingWorker> logger)
    {
        _queue = queue;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            Guid documentId;

            try
            {
                documentId =
                    await _queue.DequeueAsync(
                        stoppingToken);
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }

            try
            {
                using var scope =
                    _scopeFactory.CreateScope();

                var dbContext =
                    scope.ServiceProvider
                        .GetRequiredService<HealthcareDbContext>();

                var document =
                    await dbContext.Documents
                        .FirstOrDefaultAsync(
                            x => x.Id == documentId,
                            stoppingToken);

                if (document == null)
                {
                    _logger.LogWarning(
                        "Document {DocumentId} was not found.",
                        documentId);

                    continue;
                }

                document.Status = "Processing";
                document.ProcessingStartedAt = DateTime.UtcNow;
                document.ProcessingCompletedAt = null;
                document.ProcessingError = null;
                document.ProcessingStartedAt = DateTime.UtcNow;
                document.ProcessingCompletedAt = null;
                document.ProcessingError = null;

                await dbContext.SaveChangesAsync(
                    stoppingToken);

                var documentService =
                    scope.ServiceProvider
                        .GetRequiredService<DocumentService>();

                await documentService.ProcessDocumentAsync(
                    documentId,
                    stoppingToken);

                _logger.LogInformation(
                    "Document {DocumentId} processing completed.",
                    documentId);
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation(
                    "Document processing worker is stopping.");

                break;
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Document {DocumentId} processing failed.",
                    documentId);

                await MarkFailedAsync(
                    documentId,
                    "Document processing failed. Check server logs for details.",
                    stoppingToken);
            }
        }
    }

    private async Task MarkFailedAsync(
        Guid documentId,
        string errorMessage,
        CancellationToken cancellationToken)
    {
        try
        {
            using var scope =
                _scopeFactory.CreateScope();

            var dbContext =
                scope.ServiceProvider
                    .GetRequiredService<HealthcareDbContext>();

            var document =
                await dbContext.Documents
                    .FirstOrDefaultAsync(
                        x => x.Id == documentId,
                        cancellationToken);

            if (document == null)
            {
                return;
            }

            document.Status = "Failed";
            document.ProcessingCompletedAt = DateTime.UtcNow;
            document.ProcessingError = errorMessage;
            document.ProcessingCompletedAt = DateTime.UtcNow;
            document.ProcessingError = errorMessage;

            await dbContext.SaveChangesAsync(
                cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Unable to mark document {DocumentId} as Failed.",
                documentId);
        }
    }
}




