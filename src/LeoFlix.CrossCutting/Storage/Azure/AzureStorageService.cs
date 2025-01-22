using Azure.Storage.Blobs;

namespace LeoFlix.CrossCutting.Storage.Azure;

public class AzureStorageService : IStorageService
{
    private readonly BlobContainerClient _blobContainerClient;

    public AzureStorageService(BlobServiceClient blobServiceClient)
    {
        _blobContainerClient = blobServiceClient.GetBlobContainerClient("videos");

        _blobContainerClient.CreateIfNotExistsAsync().GetAwaiter().GetResult();
    }

    public async Task Upload(UploadFilesInput uploadFilesInput)
    {
        foreach (var (fileName, memoryStream) in uploadFilesInput.Files)
            await _blobContainerClient.UploadBlobAsync(fileName, memoryStream);
    }

    public async Task<Stream?> Download(string path)
    {
        var blobDownloadInfo = await _blobContainerClient.GetBlobClient(path).DownloadAsync();

        return blobDownloadInfo?.Value.Content;
    }
}