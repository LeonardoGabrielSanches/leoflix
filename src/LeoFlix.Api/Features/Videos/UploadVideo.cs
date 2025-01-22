using System.Threading.Channels;
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

        private static async Task<IResult> Handler(
            [FromForm] Request request,
            Channel<VideoDispatch> videoDispatchChannel)
        {
            if (request.Video.Length == 0)
                return Results.BadRequest("No file uploaded.");

            var uploadPath = Path.Combine(Constants.VideoDirectory, request.Video.FileName);

            await using (var stream = new FileStream(uploadPath, FileMode.Create))
            {
                await request.Video.CopyToAsync(stream);
            }

            await videoDispatchChannel.Writer.WriteAsync(new VideoDispatch(uploadPath));

            return Results.Accepted();
        }
    }
}

