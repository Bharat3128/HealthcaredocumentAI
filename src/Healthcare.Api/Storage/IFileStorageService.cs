namespace Healthcare.Api.Storage;

public interface IFileStorageService
{
    Task SaveAsync(
        Guid documentId,
        Stream fileStream,
        CancellationToken cancellationToken = default);

    Task<Stream> OpenReadAsync(
        Guid documentId,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        Guid documentId,
        CancellationToken cancellationToken = default);
}
