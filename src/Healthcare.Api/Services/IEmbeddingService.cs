namespace Healthcare.Api.Services;

public interface IEmbeddingService
{
    Task<float[]> GenerateEmbeddingAsync(
        string? text,
        CancellationToken cancellationToken = default);

    Task<List<float[]>> GenerateEmbeddingsAsync(
        IReadOnlyList<string> texts,
        CancellationToken cancellationToken = default);

    double CosineSimilarity(
        float[] first,
        float[] second);
}
