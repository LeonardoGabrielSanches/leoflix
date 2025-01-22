namespace LeoFlix.CrossCutting.Storage;

public record UploadFilesInput(Dictionary<string, MemoryStream> Files);

public interface IStorageService
{
    Task Upload(UploadFilesInput uploadFilesInput);
    Task<Stream?> Download(string path);
}