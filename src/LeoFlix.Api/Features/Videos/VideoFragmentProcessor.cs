using System.Diagnostics;
using System.Threading.Channels;
using LeoFlix.CrossCutting.Storage;

namespace LeoFlix.Api.Features.Videos;

public sealed record VideoDispatch(string Path);

public class VideoFragmentProcessor(
    Channel<VideoDispatch> videoDispatchChannel,
    IServiceScopeFactory scopeFactory) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var videoDispatch in videoDispatchChannel.Reader.ReadAllAsync(stoppingToken))
        {
            using var scope = scopeFactory.CreateScope();
            var fragmentProcessor = scope.ServiceProvider.GetRequiredService<FragmentProcessor>();

            await fragmentProcessor.Handle(videoDispatch, stoppingToken);
        }
    }
}

internal sealed class FragmentProcessor(
    [FromKeyedServices(KeyedServicesConstants.AzureStorageServiceKey)]
    IStorageService storageService)
{
    public async Task Handle(VideoDispatch videoDispatch, CancellationToken stoppingToken)
    {
        var videoName = Path.GetFileNameWithoutExtension(videoDispatch.Path);
        var outputPath = Path.Combine(Constants.VideoDirectory, videoName);

        Directory.CreateDirectory(outputPath);

        var ffmpegCommand = $"-i \"{videoDispatch.Path}\" -codec: copy -start_number 0 -hls_time 15 -hls_list_size 0 " +
                            $"-hls_base_url \"/api/video/fragment/{videoName}/\" -hls_segment_filename \"{outputPath}/segment%03d.ts\" " +
                            $"-f hls \"{outputPath}/{Constants.OutputFileName}\"";

        var processStartInfo = new ProcessStartInfo
        {
            FileName = "ffmpeg",
            Arguments = ffmpegCommand,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        var process = Process.Start(processStartInfo);
        if (process is null)
            throw new Exception("Invalid fragment process");

        await process.WaitForExitAsync(stoppingToken);

        File.Delete(videoDispatch.Path);

        var files = Directory.GetFiles(outputPath);

        var fileStreams = new Dictionary<string, MemoryStream>();

        foreach (var file in files)
        {
            await using var fileStream = new FileStream(file, FileMode.Open, FileAccess.Read);

            var memoryStream = new MemoryStream();
            await fileStream.CopyToAsync(memoryStream, stoppingToken);
            memoryStream.Position = 0;
            fileStreams.Add(Path.Combine(videoName, Path.GetFileName(file)), memoryStream);
        }

        await storageService.Upload(new UploadFilesInput(fileStreams));

        foreach (var file in files)
            File.Delete(file);

        Directory.Delete(outputPath);
    }
}