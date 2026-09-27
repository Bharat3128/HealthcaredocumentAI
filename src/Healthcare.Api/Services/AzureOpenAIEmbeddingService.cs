using Azure.AI.OpenAI;
using Azure.Identity;
using Healthcare.Api.Options;
using Microsoft.Extensions.Options;
using OpenAI.Embeddings;

namespace Healthcare.Api.Services;

public class AzureOpenAIEmbeddingService : IEmbeddingService
{
    private readonly EmbeddingClient _embeddingClient;

    public AzureOpenAIEmbeddingService(
        IOptions<AzureOpenAIOptions> options)
    {
        var settings = options.Value;

        if (string.IsNullOrWhiteSpace(settings.Endpoint))
        {
            throw new InvalidOperationException(
                "Azure OpenAI endpoint is not configured.");
        }

        if (string.IsNullOrWhiteSpace(settings.EmbeddingDeployment))
        {
            throw new InvalidOperationException(
                "Azure OpenAI embedding deployment is not configured.");
        }

        var azureClient = new AzureOpenAIClient(
            new Uri(settings.Endpoint),
            new DefaultAzureCredential());

        _embeddingClient =
            azureClient.GetEmbeddingClient(
                settings.EmbeddingDeployment);
    }

    public async Task<float[]> GenerateEmbeddingAsync(
        string? text,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return Array.Empty<float>();
        }

        var response =
            await _embeddingClient.GenerateEmbeddingAsync(
                text,
                cancellationToken: cancellationToken);

        return response.Value
            .ToFloats()
            .ToArray();
    }

    public async Task<List<float[]>> GenerateEmbeddingsAsync(
        IReadOnlyList<string> texts,
        CancellationToken cancellationToken = default)
    {
        var results =
            new List<float[]>(texts.Count);

        foreach (var text in texts)
        {
            results.Add(
                await GenerateEmbeddingAsync(
                    text,
                    cancellationToken));
        }

        return results;
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
