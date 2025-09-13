using FileHub.Application.Abstractions.Services;
using FileHub.Core.Common;
using FileHub.Storage.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FileHub.Storage;

internal class FileStorageService(ILogger<FileStorageService> logger,
                                  IOptions<FileStorageOptions> options) : IFileStorageService
{
    public async Task<Result<string>> SaveAsync(Stream file, string extension, CancellationToken cancellationToken = default)
    {
        try
        {
            var fileName = $"{DateTime.UtcNow:yyyyMMdd}_{Guid.NewGuid()}{extension}";

            var path = Path.Combine(options.Value.ContentDirectory, fileName);

            await using var stream = new FileStream(path, FileMode.Create);

            await file.CopyToAsync(stream, cancellationToken);

            return Result<string>.Success(path);
        }
        catch (Exception e)
        {
           logger.LogError(e, "Error while saving file.");

            return Result<string>.InternalError(e);
        }
    }

    public Task<Result<Stream>> OpenReadStreamAsync(string path, CancellationToken cancellationToken = default)
    {
        try
        {
            var stream = new FileStream(path,
                                        FileMode.Open,
                                        FileAccess.Read,
                                        FileShare.Read,
                                        options.Value.FileStreamBufferSize,
                                        useAsync: true);

            return Task.FromResult(Result<Stream>.Success(stream));
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error while opening file at Path '{Path}'.", path);

            return Task.FromResult(Result<Stream>.InternalError(e));
        }
    }
}
