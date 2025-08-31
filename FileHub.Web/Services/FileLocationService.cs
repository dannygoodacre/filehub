using FileHub.Application.Abstractions.Services;
using FileHub.Web.Configuration;
using Microsoft.Extensions.Options;

namespace FileHub.Web.Services;

internal class FileLocationService(IOptions<WebOptions> options) : IFileLocationService
{
    public string GetFileAccessLocation(string fileId) => $"{options.Value.RootUrl}/files/{fileId}";
}
