using System.Text.Json;
using Healthcare.Api.Data;
using Healthcare.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Healthcare.Api.Services;

public class SemanticSearchResult
{
    public int ChunkId { get; set; }

    public int ChunkIndex { get; set; }

    public string Content { get; set; } = string.Empty;

    public double Score { get; set; }
}

public class DocumentRetrievalService
{
    private readonly HealthcareDbContext _dbContext;
    private readonly IEmbeddingService _embeddingService;

    public DocumentRetrievalService(
        HealthcareDbContext dbContext,
        IEmbeddingService embeddingService)
    {
        _dbContext = dbContext;
        _embeddingService = embeddingService;
    }

    public async Task<List<DocumentChunk>> SearchAsync(
        Guid documentId,
        string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return new List<DocumentChunk>();
        }

        var words = query
            .ToLowerInvariant()
            .Split(' ', StringSplitOptions.RemoveEmptyEntries);

        var chunks = await _dbContext.DocumentChunks
            .AsNoTracking()
            .Where(chunk => chunk.DocumentId == documentId)
            .ToListAsync();

        return chunks
            .Select(chunk => new
            {
                Chunk = chunk,
                Score = words.Count(word =>
                    chunk.Content.Contains(
                        word,
                        StringComparison.OrdinalIgnoreCase))
            })
            .Where(result => result.Score > 0)
            .OrderByDescending(result => result.Score)
            .Take(5)
            .Select(result => result.Chunk)
            .ToList();
    }

    public async Task<List<SemanticSearchResult>> SemanticSearchAsync(
        Guid documentId,
        string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return new List<SemanticSearchResult>();
        }

        var queryEmbedding =
            await _embeddingService.GenerateEmbeddingAsync(query);

        var chunks = await _dbContext.DocumentChunks
            .AsNoTracking()
            .Where(chunk =>
                chunk.DocumentId == documentId &&
                chunk.EmbeddingJson != null)
            .ToListAsync();

        var results = new List<SemanticSearchResult>();

        foreach (var chunk in chunks)
        {
            var chunkEmbedding =
                JsonSerializer.Deserialize<float[]>(
                    chunk.EmbeddingJson!);

            if (chunkEmbedding == null)
            {
                continue;
            }

            var score =
                _embeddingService.CosineSimilarity(
                    queryEmbedding,
                    chunkEmbedding);

            results.Add(new SemanticSearchResult
            {
                ChunkId = chunk.Id,
                ChunkIndex = chunk.ChunkIndex,
                Content = chunk.Content,
                Score = score
            });
        }

        return results
            .OrderByDescending(result => result.Score)
            .Take(5)
            .ToList();
    }
}


