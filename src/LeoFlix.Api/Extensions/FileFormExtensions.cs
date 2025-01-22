using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;

namespace LeoFlix.Api.Extensions;

public static class FileFormExtensions
{
    public static IServiceCollection AddFileForm(this IServiceCollection services)
    {
        services.Configure<KestrelServerOptions>(options =>
        {
            options.Limits.MaxRequestBodySize = 200 * 1024 * 1024; // 200 MB
        });

        services.Configure<FormOptions>(options =>
        {
            options.MultipartBodyLengthLimit = 200 * 1024 * 1024; // 200 MB
        });

        return services;
    }
}