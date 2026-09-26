namespace Healthcare.Api.Storage;

public class LocalFileStorageService : IFileStorageService
{
    private readonly string _uploadsFolder;

    public LocalFileStorageService(
        IWebHostEnvironment environment)
    {
        _uploadsFolder = Path.Combine(
            environment.ContentRootPath,
            "Uploads");

        Directory.CreateDirectory(_uploadsFolder);
    }

    public async Task SaveAsync(
        Guid documentId,
        Stream fileStream,
        CancellationToken cancellationToken = default)
    {
        var filePath = GetFilePath(documentId);

        await using var outputStream =
            new FileStream(
                filePath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                81920,
                useAsync: true);

        await fileStream.CopyToAsync(
            outputStream,
            cancellationToken);
    }

    public Task<Stream> OpenReadAsync(
        Guid documentId,
        CancellationToken cancellationToken = default)
    {
        var filePath = GetFilePath(documentId);

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException(
                "Document file was not found.",
                filePath);
        }

        Stream stream =
            new FileStream(
                filePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                81920,
                useAsync: true);

        return Task.FromResult(stream);
    }

    public Task DeleteAsync(
        Guid documentId,
        CancellationToken cancellationToken = default)
    {
        var filePath = GetFilePath(documentId);

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        return Task.CompletedTask;
    }

    private string GetFilePath(Guid documentId)
    {
        return Path.Combine(
            _uploadsFolder,
            $"{documentId}.pdf");
    }
}
