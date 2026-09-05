using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using LibraryApi.Services.Interfaces;
using LibraryApi.Settings;
using Microsoft.Extensions.Options;

namespace LibraryApi.Services;

public class BlobStorageService : IBlobStorageService
{
    private readonly BlobContainerClient _containerClient;

    public BlobStorageService(IOptions<AzureBlobStorageSettings> options)
    {
        AzureBlobStorageSettings settings = options.Value;

        if (string.IsNullOrWhiteSpace(settings.ConnectionString))
        {
            throw new InvalidOperationException(
                "Azure Blob Storage connection string is not configured.");
        }

        if (string.IsNullOrWhiteSpace(settings.ContainerName))
        {
            throw new InvalidOperationException(
                "Azure Blob Storage container name is not configured.");
        }

        BlobServiceClient blobServiceClient =
            new BlobServiceClient(settings.ConnectionString);

        _containerClient =
            blobServiceClient.GetBlobContainerClient(settings.ContainerName);
    }

    public async Task<string> UploadBookImageAsync(IFormFile file)
    {
        string extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        string blobName = $"{Guid.NewGuid()}{extension}";

        BlobClient blobClient = _containerClient.GetBlobClient(blobName);

        await using Stream stream = file.OpenReadStream();

        BlobUploadOptions options = new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders
            {
                ContentType = file.ContentType
            }
        };

        await blobClient.UploadAsync(stream, options);

        return blobClient.Uri.ToString();
    }

    public async Task DeleteAsync(string imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
        {
            return;
        }

        Uri uri = new Uri(imageUrl);

        string blobName = Path.GetFileName(uri.LocalPath);

        BlobClient blobClient = _containerClient.GetBlobClient(blobName);

        await blobClient.DeleteIfExistsAsync();
    }
}