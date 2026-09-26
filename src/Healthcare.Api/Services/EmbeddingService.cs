using System.Net.Http.Json;
using Healthcare.Api.Options;
using Microsoft.Extensions.Options;

namespace Healthcare.Api.Services;

public class EmbeddingService
{
    private readonly HttpClient _httpClient;
    private readonly OllamaOptions _options;

    [Microsoft.Extensions.DependencyInjection.ActivatorUtilitiesConstructor]
    public EmbeddingService(
        HttpClient httpClient,
        IOptions<OllamaOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public EmbeddingService(HttpClient httpClient)
        : this(
            httpClient,
            Microsoft.Extensions.Options.Options.Create(new OllamaOptions()))
    {
    }

    public async Task<float[]> GenerateEmbeddingAsync(
        string? text,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return Array.Empty<float>();
        }

        var embeddings =
            await GenerateEmbeddingsAsync(
                new List<string> { text },
                cancellationToken);

        return embeddings.FirstOrDefault()
            ?? Array.Empty<float>();
    }

    public async Task<List<float[]>> GenerateEmbeddingsAsync(
        IReadOnlyList<string> texts,
        CancellationToken cancellationToken = default)
    {
        if (texts.Count == 0)
        {
            return new List<float[]>();
        }

        var request = new
        {
            model = _options.EmbeddingModel,
            input = texts
        };

        var response =
            await _httpClient.PostAsJsonAsync(
                "/api/embed",
                request,
                cancellationToken);

        response.EnsureSuccessStatusCode();

        var result =
            await response.Content
                .ReadFromJsonAsync<OllamaEmbeddingResponse>(
                    cancellationToken);

        return result?.Embeddings
            .Select(vector => vector.ToArray())
            .ToList()
            ?? new List<float[]>();
    }

    public double CosineSimilarity(
        float[] first,
        float[] second)
    {
        if (first.Length == 0 ||
            second.Length == 0 ||
            first.Length != second.Length)
        {
            return 0;
        }

        double dotProduct = 0;
        double firstMagnitude = 0;
        double secondMagnitude = 0;

        for (var i = 0; i < first.Length; i++)
        {
            dotProduct += first[i] * second[i];
            firstMagnitude += first[i] * first[i];
            secondMagnitude += second[i] * second[i];
        }

        if (firstMagnitude == 0 ||
            secondMagnitude == 0)
        {
            return 0;
        }

        return dotProduct /
            (Math.Sqrt(firstMagnitude) *
             Math.Sqrt(secondMagnitude));
    }
}

public class OllamaEmbeddingResponse
{
    public List<List<float>> Embeddings { get; set; } = new();
}






