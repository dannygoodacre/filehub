using FileHub.Application.Abstractions.Services;
using FileHub.Utilities.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Sqids;

namespace FileHub.Utilities.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddUtilities(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IValidateOptions<SqidsOptions>, SqidsOptionsValidator>();

        services
            .AddOptions<SqidsOptions>()
            .Bind(configuration.GetSection("Sqids"))
            .ValidateOnStart();

        services.AddSingleton<SqidsEncoder<int>>(x =>
        {
            var options = x.GetRequiredService<IOptions<SqidsOptions>>();

            return new SqidsEncoder<int>(options.Value);
        });

        services.AddScoped<IIdEncoderService<int>, IdEncoderService<int>>();

        return services;
    }
}
