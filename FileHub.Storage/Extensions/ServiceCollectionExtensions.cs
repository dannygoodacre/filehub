using FileHub.Application.Abstractions.Services;
using FileHub.Storage.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace FileHub.Storage.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddStorage(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IValidateOptions<FileStorageOptions>, FileStorageOptionsValidator>();

        services
            .AddOptions<FileStorageOptions>()
            .Bind(configuration)
            .ValidateOnStart();

        services.AddScoped<IFileStorageService, FileStorageService>();

        return services;
    }
}
