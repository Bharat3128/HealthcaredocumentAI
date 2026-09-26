using Healthcare.Api.Models;
using Healthcare.Api.Options;
using Microsoft.Extensions.Options;

namespace Healthcare.Api.Services;

public class DocumentChunkingService
{
    private readonly ChunkingOptions _options;

    public DocumentChunkingService(
        IOptions<ChunkingOptions> options)
    {
        _options = options.Value;
    }

    public List<DocumentChunk> CreateChunks(
        Guid documentId,
        string? text)
    {
        var chunks = new List<DocumentChunk>();

        if (string.IsNullOrWhiteSpace(text))
        {
            return chunks;
        }

        var chunkSize = _options.ChunkSize;
        var overlap = _options.Overlap;

        if (chunkSize <= 0)
        {
            throw new InvalidOperationException(
                "ChunkSize must be greater than zero.");
        }

        if (overlap < 0 || overlap >= chunkSize)
        {
            throw new InvalidOperationException(
                "Overlap must be greater than or equal to zero and less than ChunkSize.");
        }

        var start = 0;
        var chunkIndex = 0;

        while (start < text.Length)
        {
            var length = Math.Min(
                chunkSize,
                text.Length - start);

            var content = text
                .Substring(start, length)
                .Trim();

            if (!string.IsNullOrWhiteSpace(content))
            {
                chunks.Add(new DocumentChunk
                {
                    DocumentId = documentId,
                    ChunkIndex = chunkIndex,
                    Content = content
                });

                chunkIndex++;
            }

            if (start + length >= text.Length)
            {
                break;
            }

            start += chunkSize - overlap;
        }

        return chunks;
    }
}
