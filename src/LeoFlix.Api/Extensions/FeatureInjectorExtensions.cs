using System.Threading.Channels;
using LeoFlix.Api.Features.Videos;
using LeoFlix.CrossCutting.Storage;
using LeoFlix.CrossCutting.Storage.Azure;

namespace LeoFlix.Api.Extensions;

public static class FeatureInjectorExtensions
{
    public static IServiceCollection AddFeatures(this IServiceCollection services)
    {
        services.AddScoped<FragmentProcessor>();
        services.AddHostedService<VideoFragmentProcessor>();
        services.AddSingleton(_ => Channel.CreateBounded<VideoDispatch>(new BoundedChannelOptions(100)
        {
            FullMode = BoundedChannelFullMode.Wait
        }));

        services.AddKeyedScoped<IStorageService, AzureStorageService>(KeyedServicesConstants.AzureStorageServiceKey);

        return services;
    }
}