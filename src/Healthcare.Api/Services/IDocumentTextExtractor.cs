namespace Healthcare.Api.Services;

public interface IDocumentTextExtractor
{
    Task<string> ExtractTextAsync(
        Stream stream,
        CancellationToken cancellationToken = default);
}
