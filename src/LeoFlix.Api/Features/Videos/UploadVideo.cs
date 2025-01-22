using System.IO.Compression;
using System.Threading.Channels;
using Flunt.Validations;
using LeoFlix.Api.Shared;
using Microsoft.AspNetCore.Mvc;

namespace LeoFlix.Api.Features.Videos;

public static class UploadVideo
{
    public record Request(IFormFile Video);

    public record Response(string VideoUrl);

    public class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("api/video/upload", Handler)
                .WithOpenApi()
                .WithTags("Video")
                .DisableAntiforgery();
        }

        private class UploadValidator : Contract<Request>
        {
            public UploadValidator(Request request)
            {
                Requires()
                    .IsNotNull(request.Video, "Video", "No file uploaded.")
                    .IsTrue(BeAValidZipFile(request.Video), "Video", "The uploaded file is not a valid ZIP file.");
            }

            private static bool BeAValidZipFile(IFormFile file)
            {
                return file.ContentType == "application/zip";
            }
        }

        public static async Task<IResult> Handler(
            [FromForm] Request request,
            Channel<VideoDispatch> videoDispatchChannel)
        {
            var validation = new UploadValidator(request);
            if (!validation.IsValid)
                return Results.BadRequest(validation.Notifications);

            // Open the ZIP file from the request stream
            using (var zipArchive = new ZipArchive(request.Video.OpenReadStream()))
            {
                foreach (var entry in zipArchive.Entries)
                {
                    // Create the file path for each entry (inside the VideoDirectory)
                    var extractedFilePath = Path.Combine(Constants.VideoDirectory, entry.FullName);

                    // Ensure the directory exists for extraction
                    var directory = Path.GetDirectoryName(extractedFilePath);
                    if (directory != null && !Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }

                    // Extract the file directly from the ZIP entry stream
                    await using (var entryStream = entry.Open())
                    await using (var fileStream = new FileStream(extractedFilePath, FileMode.Create))
                    {
                        await entryStream.CopyToAsync(fileStream);
                    }

                    // Process the extracted file (send it to the video dispatch channel)
                    await videoDispatchChannel.Writer.WriteAsync(new VideoDispatch(extractedFilePath));
                }
            }

            return Results.Accepted();
        }
    }
}