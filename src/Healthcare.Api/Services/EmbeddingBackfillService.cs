using System.Text.Json;
using Healthcare.Api.Data;
using Healthcare.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Healthcare.Api.Services;

public class EmbeddingBackfillService
{
    private readonly HealthcareDbContext _dbContext;
    private readonly EmbeddingService _embeddingService;

    public EmbeddingBackfillService(
        HealthcareDbContext dbContext,
        EmbeddingService embeddingService)
    {
        _dbContext = dbContext;
        _embeddingService = embeddingService;
    }

    public async Task<int> GenerateEmbeddingsAsync(Guid documentId)
    {
        var allChunks = await _dbContext.DocumentChunks
            .Where(x => x.DocumentId == documentId)
            .OrderBy(x => x.ChunkIndex)
            .ToListAsync();

        var chunks = allChunks
            .Where(NeedsEmbedding)
            .ToList();

        const int batchSize = 64;
        var updated = 0;

        for (var start = 0; start < chunks.Count; start += batchSize)
        {
            var batch = chunks
                .Skip(start)
                .Take(batchSize)
                .ToList();

            var embeddings =
                await _embeddingService.GenerateEmbeddingsAsync(
                    batch.Select(x => x.Content).ToList());

            for (var i = 0;
                 i < batch.Count && i < embeddings.Count;
                 i++)
            {
                batch[i].EmbeddingJson =
                    JsonSerializer.Serialize(embeddings[i]);

                updated++;
            }

            await _dbContext.SaveChangesAsync();
        }

        return updated;
    }

    private static bool NeedsEmbedding(DocumentChunk chunk)
    {
        if (string.IsNullOrWhiteSpace(chunk.EmbeddingJson))
        {
            return true;
        }

        try
        {
            var existing =
                JsonSerializer.Deserialize<float[]>(
                    chunk.EmbeddingJson);

            return existing == null ||
                   existing.Length != 768;
        }
        catch
        {
            return true;
        }
    }
}
