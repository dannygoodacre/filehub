using FileHub.Application.Commands;
using FileHub.Application.Queries;
using Microsoft.Extensions.DependencyInjection;

namespace FileHub.Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAddFile, AddFile>();
        services.AddScoped<IAddCategory, AddCategory>();

        services.AddScoped<IGetFileContent, GetFileContent>();
        services.AddScoped<IGetFileMetadata, GetFileMetadata>();
        services.AddScoped<IGetPaginatedFileMetadata, GetPaginatedFilesMetadata>();
        services.AddScoped<IGetFilePageCount, GetFilePageCount>();
        services.AddScoped<IGetAllCategories, GetAllCategories>();

        return services;
    }
}
