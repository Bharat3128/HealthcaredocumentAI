using Azure;
using Azure.AI.DocumentIntelligence;
using Azure.Identity;
using Healthcare.Api.Options;
using Microsoft.Extensions.Options;

namespace Healthcare.Api.Services;

public class AzureDocumentIntelligenceService : IDocumentTextExtractor
{
    private readonly DocumentIntelligenceClient _client;
    private readonly DocumentIntelligenceOptions _options;

    public AzureDocumentIntelligenceService(
        IOptions<DocumentIntelligenceOptions> options)
    {
        _options = options.Value;

        if (string.IsNullOrWhiteSpace(_options.Endpoint))
        {
            throw new InvalidOperationException(
                "Document Intelligence endpoint is not configured.");
        }

        _client = new DocumentIntelligenceClient(
            new Uri(_options.Endpoint),
            new DefaultAzureCredential());
    }

    public async Task<string> ExtractTextAsync(
        Stream stream,
        CancellationToken cancellationToken = default)
    {
        if (stream == null || !stream.CanRead)
            throw new InvalidDataException(
                "The document stream cannot be read.");

        using var memoryStream = new MemoryStream();

        await stream.CopyToAsync(
            memoryStream,
            cancellationToken);

        var documentData =
            new BinaryData(memoryStream.ToArray());

        var operation =
            await _client.AnalyzeDocumentAsync(
                WaitUntil.Completed,
                _options.ModelId,
                documentData,
                cancellationToken);

        var text = operation.Value.Content?.Trim();

        return string.IsNullOrWhiteSpace(text)
            ? "No readable text was found in this document."
            : text;
    }
}
