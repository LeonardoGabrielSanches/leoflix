using LeoFlix.Api.Shared;
using LeoFlix.CrossCutting.Storage;

namespace LeoFlix.Api.Features.Videos;

public static class GetFragment
{
    public class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("api/video/fragment/{videoName}/{fragmentName}", Handler)
                .WithOpenApi()
                .WithTags("Video")
                .DisableAntiforgery();
        }

        private async Task<IResult> Handler(
            [FromKeyedServices(KeyedServicesConstants.AzureStorageServiceKey)]
            IStorageService storageService,
            string videoName,
            string fragmentName)
        {
            var stream = await storageService.Download(Path.Combine(videoName, fragmentName));

            return stream is null
                ? Results.NotFound("Fragment not found.")
                : Results.File(stream, "video/MP2T");
        }
    }
}