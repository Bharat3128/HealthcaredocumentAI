using Azure.Identity;
using Azure.Storage.Blobs;
using Healthcare.Api.Options;
using Microsoft.Extensions.Options;

namespace Healthcare.Api.Storage;

public class AzureBlobStorageService : IFileStorageService
{
    private readonly BlobContainerClient _containerClient;

    public AzureBlobStorageService(IOptions<AzureStorageOptions> options)
    {
        var settings = options.Value;

        if (string.IsNullOrWhiteSpace(settings.ServiceUri))
        {
            throw new InvalidOperationException(
                "Azure Storage ServiceUri is not configured.");
        }

        if (string.IsNullOrWhiteSpace(settings.ContainerName))
        {
            throw new InvalidOperationException(
                "Azure Storage ContainerName is not configured.");
        }

        var credential = new DefaultAzureCredential();

        var serviceClient = new BlobServiceClient(
            new Uri(settings.ServiceUri),
            credential);

        _containerClient =
            serviceClient.GetBlobContainerClient(settings.ContainerName);
    }

    public async Task SaveAsync(
        Guid documentId,
        Stream fileStream,
        CancellationToken cancellationToken = default)
    {
        await _containerClient.CreateIfNotExistsAsync(
            cancellationToken: cancellationToken);

        var blobClient =
            _containerClient.GetBlobClient($"{documentId}.pdf");

        await blobClient.UploadAsync(
            fileStream,
            overwrite: false,
            cancellationToken);
    }

    public async Task<Stream> OpenReadAsync(
        Guid documentId,
        CancellationToken cancellationToken = default)
    {
        var blobClient =
            _containerClient.GetBlobClient($"{documentId}.pdf");

        var response =
            await blobClient.DownloadStreamingAsync(
                cancellationToken: cancellationToken);

        return response.Value.Content;
    }

    public async Task DeleteAsync(
        Guid documentId,
        CancellationToken cancellationToken = default)
    {
        var blobClient =
            _containerClient.GetBlobClient($"{documentId}.pdf");

        await blobClient.DeleteIfExistsAsync(
            cancellationToken: cancellationToken);
    }
}
