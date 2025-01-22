using LeoFlix.Api.Shared;
using LeoFlix.CrossCutting.Storage;

namespace LeoFlix.Api.Features.Videos;

public static class GetVideo
{
    public class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("api/video/playlist/{videoName}", Handler)
                .WithOpenApi()
                .WithTags("Video")
                .DisableAntiforgery();
        }

        public static async Task<IResult> Handler(
            [FromKeyedServices(KeyedServicesConstants.AzureStorageServiceKey)]
            IStorageService storageService,
            string videoName)
        {
            var stream = await storageService.Download(Path.Combine(videoName, Constants.OutputFileName));

            return stream is null
                ? Results.NotFound("Playlist not found.")
                : Results.File(stream, "application/vnd.apple.mpegurl");
        }
    }
}