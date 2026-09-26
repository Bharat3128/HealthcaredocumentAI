namespace Healthcare.Api.Models;

public class DocumentChunk
{
    public int Id { get; set; }

    public Guid DocumentId { get; set; }

    public int ChunkIndex { get; set; }

    public string Content { get; set; } = string.Empty;

    public string? EmbeddingJson { get; set; }
}
